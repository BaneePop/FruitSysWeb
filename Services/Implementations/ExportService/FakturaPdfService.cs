using FruitSysWeb.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    /// <summary>
    /// Generiše PDF fakture i profakture na srpskom ili engleskom jeziku.
    /// Domaće fakture: RSD iznosi, transport sekcija, domaći bankovni računi.
    /// Inostrane fakture: EUR iznosi, transport sekcija, SWIFT/IBAN instrukcije + PDV oslobođenje.
    /// </summary>
    public class FakturaPdfService
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");
        private static readonly string LogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo.png");

        // ODETTA bankovni podaci za domaće plaćanje
        private const string OdettaAikBanka = "105-14819-95";
        private const string OdettaHalkbank = "155-30407-66";

        // SWIFT/IBAN podaci za inostrana plaćanja
        private const string SwiftCorrespondent = "SOGEFRPP – SOCIETE GENERALE";
        private const string SwiftCorrespondentAddr = "F-92978 PARIS FRANCE";
        private const string SwiftAccCode = "AIKBRS22";
        private const string SwiftAccInstitution = "AIK BANKA AD, BEOGRAD";
        private const string SwiftAccAddr = "BULEVAR MIHAILA PUPINA 115D\n11070 NOVI BEOGRAD, REPUBLIKA SRBIJA";
        private const string SwiftBeneficiaryIban = "RS35105057012001081134";
        private const string SwiftBeneficiaryName = "ODETTA DOO";
        private const string SwiftBeneficiaryAddr = "KRALJA DRAGUTINA 5, 7/31\nŠABAC\nREPUBLIKA SRBIJA";
        private const string PdvOslobodjenje = "Oslobođeno plaćanja PDV na osnovu člana 24. stav 1. tačka 2. Zakona o porezu na dodatu vrednost.";

        // ═══════════════════════════════════════════════════════════════
        // JAVNE METODE
        // ═══════════════════════════════════════════════════════════════

        public byte[] GenerisiFakturu(FakturaDetaljiModel model, bool naEngleskom = false)
            => BuildDocument(model, naEngleskom, isProfaktura: false);

        public byte[] GenerisiProfakturu(FakturaDetaljiModel model, bool naEngleskom = false)
            => BuildDocument(model, naEngleskom, isProfaktura: true);

        // ═══════════════════════════════════════════════════════════════
        // BUILDER
        // ═══════════════════════════════════════════════════════════════

        private byte[] BuildDocument(FakturaDetaljiModel model, bool en, bool isProfaktura)
        {
            var labels = en ? EnLabels : SrLabels;
            var naslov = isProfaktura
                ? (en ? "PRO-FORMA INVOICE" : "PROFAKTURA")
                : (en ? "INVOICE" : "FAKTURA");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                    page.Header().Element(c => BuildHeader(c, model, naslov, labels, en));
                    page.Content().Element(c => BuildContent(c, model, labels, en));
                    page.Footer().Element(c => BuildFooter(c, model));
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        // ═══════════════════════════════════════════════════════════════
        // HEADER — zaglavlje firme + naslov + podaci fakture i kupca
        // ═══════════════════════════════════════════════════════════════

        private void BuildHeader(IContainer container, FakturaDetaljiModel model, string naslov, Labels l, bool en)
        {
            container.Column(col =>
            {
                // ─── Firma zaglavlje (beli, bez boje pozadine) ───
                col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(8).Row(row =>
                {
                    if (File.Exists(LogoPath))
                    {
                        row.ConstantItem(100).PaddingRight(12).AlignMiddle()
                            .Image(LogoPath).FitArea();
                    }

                    row.RelativeItem().AlignMiddle().Column(c =>
                    {
                        c.Item().Text("ODETTA DOO").FontSize(14).Bold().FontColor(Colors.Black);
                        c.Item().Text("Kralja Dragutina 5, 7/31, 15000 Šabac, Srbija")
                            .FontSize(8).FontColor(Colors.Grey.Darken2);
                        c.Item().Text("PIB: 102679301  |  MB: 17392344")
                            .FontSize(8).FontColor(Colors.Grey.Darken2);
                        c.Item().Text("Tel: 015/7511-299  |  www.odetta.rs")
                            .FontSize(8).FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(140).AlignMiddle().AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text(naslov)
                            .FontSize(18).Bold().FontColor(Colors.Black);
                        c.Item().AlignRight().Text($"No: {model.Faktura.Sifra}")
                            .FontSize(10).SemiBold().FontColor(Colors.Black);
                        c.Item().AlignRight().Text($"{l.Date}: {model.Faktura.Datum.ToString("dd.MM.yyyy", SrFormat)}")
                            .FontSize(9).FontColor(Colors.Grey.Darken2);
                    });
                });

                col.Item().PaddingVertical(4);

                // ─── Podaci fakture i kupca (2 kolone) ───
                col.Item().Row(row =>
                {
                    // Leva kolona — podaci dokumenta
                    row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                        .Padding(8).Column(c =>
                        {
                            c.Item().Text(l.DocumentInfo).FontSize(9).Bold()
                                .FontColor(Colors.Black);
                            c.Item().PaddingTop(4);
                            DocRow(c, l.InvoiceNo, model.Faktura.Sifra);
                            DocRow(c, l.Date, model.Faktura.Datum.ToString("dd.MM.yyyy", SrFormat));
                            if (model.JeInoKupac)
                            {
                                // Ino: samo EUR, bez ExchangeRate
                                DocRow(c, l.Currency, "EUR");
                            }
                            else
                            {
                                DocRow(c, l.Currency, "RSD");
                            }
                            if (model.Otpremnica != null)
                                DocRow(c, l.DeliveryNote, $"{model.Otpremnica.Sifra} ({model.Otpremnica.Datum:dd.MM.yyyy})");
                            if (model.Ugovor != null)
                            {
                                DocRow(c, l.ContractNo, model.Ugovor.BrojUgovora);
                                if (!string.IsNullOrEmpty(model.Ugovor.Paritet))
                                    DocRow(c, l.DeliveryTerms, model.Ugovor.Paritet);
                                if (!string.IsNullOrEmpty(model.Ugovor.Placanje))
                                    DocRow(c, l.PaymentTerms, model.Ugovor.Placanje);
                            }
                        });

                    row.ConstantItem(8);

                    // Desna kolona — podaci kupca
                    row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                        .Padding(8).Column(c =>
                        {
                            c.Item().Text(l.BillTo).FontSize(9).Bold()
                                .FontColor(Colors.Black);
                            c.Item().PaddingTop(4);

                            if (model.Kupac != null)
                            {
                                c.Item().Text(model.Kupac.Naziv).FontSize(10).Bold();
                                c.Item().Text(model.Kupac.Adresa).FontSize(9);
                                c.Item().Text($"{model.Kupac.PostanskiBroj} {model.Kupac.Mesto}").FontSize(9);
                                c.Item().Text(model.Kupac.Drzava).FontSize(9);
                                if (!string.IsNullOrEmpty(model.Kupac.PoreskiBroj))
                                    DocRow(c, l.Vat, model.Kupac.PoreskiBroj);
                                if (!string.IsNullOrEmpty(model.Kupac.MaticniBroj))
                                    DocRow(c, l.RegNo, model.Kupac.MaticniBroj);
                                if (!string.IsNullOrEmpty(model.Kupac.BrojRacuna))
                                    DocRow(c, l.AccountNo, model.Kupac.BrojRacuna);
                            }
                            else
                            {
                                c.Item().Text(model.Faktura.Komitent ?? "-").FontSize(10).Bold();
                            }
                        });
                });

                col.Item().PaddingVertical(4);
            });
        }

        // ═══════════════════════════════════════════════════════════════
        // CONTENT — transport sekcija + tabela stavki + totali + potpisi
        // ═══════════════════════════════════════════════════════════════

        private void BuildContent(IContainer container, FakturaDetaljiModel model, Labels l, bool en)
        {
            container.Column(col =>
            {
                // ─── Transport/Isporuka sekcija ───
                // Prikazati samo ako ima bar jedan neprazan podatak
                bool imaVozilo = !string.IsNullOrEmpty(model.Otpremnica?.Vozilo);
                bool imaLot = !string.IsNullOrEmpty(model.Otpremnica?.LotNaloga);
                bool imaRN = !string.IsNullOrEmpty(model.Otpremnica?.RadniNalogSifra);
                bool imaBrojPak = model.Otpremnica?.RadniNalogBrojPakovanja.HasValue == true;
                bool imaBrutoTez = model.Otpremnica?.BrutoTezina.HasValue == true;
                bool prikaziTransport = imaVozilo || imaLot || imaRN || imaBrojPak || imaBrutoTez;

                if (prikaziTransport)
                {
                    col.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                        .Background(Colors.Grey.Lighten4).Padding(8).Column(c =>
                        {
                            c.Item().Text(l.TransportInfo).FontSize(9).Bold()
                                .FontColor(Colors.Black);
                            c.Item().PaddingTop(4);

                            c.Item().Row(row =>
                            {
                                row.RelativeItem().Column(left =>
                                {
                                    if (imaVozilo)
                                        DocRow(left, l.Vehicle, model.Otpremnica!.Vozilo!);
                                    if (imaLot)
                                        DocRow(left, l.Lot, model.Otpremnica!.LotNaloga!);
                                });
                                row.ConstantItem(8);
                                row.RelativeItem().Column(right =>
                                {
                                    if (imaRN)
                                        DocRow(right, l.WorkOrder, model.Otpremnica!.RadniNalogSifra!);
                                    if (imaBrojPak)
                                        DocRow(right, l.NumPackages, model.Otpremnica!.RadniNalogBrojPakovanja!.Value.ToString());
                                    if (imaBrutoTez)
                                        DocRow(right, l.GrossWeight, $"{model.Otpremnica!.BrutoTezina!.Value:N2} kg");
                                });
                            });
                        });

                    col.Item().PaddingVertical(6);
                }

                // ─── Tabela stavki ───
                col.Item().Table(table =>
                {
                    if (model.JeInoKupac)
                        DefineInoColumns(table);
                    else
                        DefineDomaceColumns(table);

                    table.Header(header =>
                    {
                        HeaderCell(header, "#", align: "center");
                        HeaderCell(header, l.Description);
                        HeaderCell(header, l.Packaging);
                        HeaderCell(header, l.Quantity, align: "right");
                        if (model.JeInoKupac)
                        {
                            HeaderCell(header, "Price EUR", align: "right");
                            HeaderCell(header, $"{l.Amount} EUR", align: "right");
                        }
                        else
                        {
                            HeaderCell(header, $"Cena RSD", align: "right");
                            HeaderCell(header, $"{l.Amount} RSD", align: "right");
                        }
                    });

                    for (int i = 0; i < model.Stavke.Count; i++)
                    {
                        var s = model.Stavke[i];
                        var bg = i % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                        DataCell(table, (i + 1).ToString(), bg, "center");
                        DataCell(table, s.ArtikalNaziv, bg);
                        DataCellPakovanje(table, FormatPakovanje(s), bg);
                        DataCell(table, s.KolicinaFormatted, bg, "right");
                        if (model.JeInoKupac)
                        {
                            DataCell(table, s.JedinicnaCenaEurFormatted, bg, "right");
                            DataCell(table, s.NetoIznosEurFormatted, bg, "right");
                        }
                        else
                        {
                            DataCell(table, s.JedinicnaCenaFormatted, bg, "right");
                            DataCell(table, s.NetoIznosFormatted, bg, "right");
                        }
                    }
                });

                col.Item().PaddingVertical(6);

                // ─── Totali ───
                col.Item().Row(row =>
                {
                    row.RelativeItem();
                    row.ConstantItem(260).Column(tc =>
                    {
                        if (model.JeInoKupac)
                        {
                            TotalRow(tc, l.NetBase, $"{model.UkupnoNetoEurFormatted} EUR");
                            TotalRow(tc, l.Total, $"{model.UkupnoBrutoEurFormatted} EUR", bold: true, bgColor: Colors.Blue.Lighten4);
                        }
                        else
                        {
                            TotalRow(tc, l.NetBase, $"{model.UkupnoNetoFormatted} RSD");
                            TotalRow(tc, $"{l.Pdv} ({model.PdvStopa:N0}%)", $"{model.UkupnoPorezFormatted} RSD");
                            TotalRow(tc, l.Total, $"{model.UkupnoBrutoFormatted} RSD", bold: true, bgColor: Colors.Blue.Lighten4);
                        }
                    });
                });

                // ─── Slovima (samo za domaće fakture u RSD) ───
                if (!model.JeInoKupac)
                {
                    col.Item().PaddingTop(6).Row(r =>
                    {
                        r.RelativeItem().Text(text =>
                        {
                            text.Span($"{l.InWords}: ").FontSize(8).SemiBold().FontColor(Colors.Grey.Darken2);
                            text.Span(IznosSlovima(model.UkupnoBruto)).FontSize(8).Italic();
                        });
                    });
                }

                col.Item().PaddingVertical(10);

                // ─── Podaci za plaćanje ───
                if (model.JeInoKupac)
                    BuildInoPaymentSection(col, l, model);
                else
                    BuildDomacePaymentSection(col, l);

                col.Item().PaddingVertical(10);

                // ─── Potpisi ───
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(l.AuthorizedBy).FontSize(9).Bold();
                        c.Item().PaddingTop(20).BorderBottom(1).BorderColor(Colors.Grey.Darken1)
                            .Width(120).Text("").FontSize(9);
                        c.Item().PaddingTop(4).Text("ODETTA DOO").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text(l.ReceivedBy).FontSize(9).Bold();
                        c.Item().AlignCenter().PaddingTop(20).BorderBottom(1)
                            .BorderColor(Colors.Grey.Darken1).Width(120).Text("").FontSize(9);
                        c.Item().AlignCenter().PaddingTop(4)
                            .Text(model.Kupac?.Naziv ?? "").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }

        // ═══════════════════════════════════════════════════════════════
        // SEKCIJE ZA PLAĆANJE
        // ═══════════════════════════════════════════════════════════════

        private static void BuildDomacePaymentSection(ColumnDescriptor col, Labels l)
        {
            col.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
            {
                c.Item().Text(l.PaymentInfo).FontSize(9).Bold().FontColor(Colors.Black);
                c.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Column(left =>
                    {
                        left.Item().Text("AIK Banka AD Beograd").FontSize(8).SemiBold();
                        left.Item().Text($"Račun: {OdettaAikBanka}").FontSize(8);
                    });
                    r.ConstantItem(12);
                    r.RelativeItem().Column(right =>
                    {
                        right.Item().Text("Halkbank AD Beograd").FontSize(8).SemiBold();
                        right.Item().Text($"Račun: {OdettaHalkbank}").FontSize(8);
                    });
                });
            });
        }

        private static void BuildInoPaymentSection(ColumnDescriptor col, Labels l, FakturaDetaljiModel model)
        {
            // Iznos u dinarima: koristimo Bruto iz fakture (uvek RSD)
            decimal iznosRsd = model.Faktura.Bruto > 0 ? model.Faktura.Bruto : model.UkupnoBruto;
            string iznosRsdStr = iznosRsd.ToString("N2", new CultureInfo("sr-Latn-RS")) + " rsd";

            col.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
            {
                c.Item().Text(l.PaymentInfo).FontSize(9).Bold().FontColor(Colors.Black);
                c.Item().PaddingTop(6);

                SwiftRow(c, "Correspodent is:", $"{SwiftCorrespondent}\n{SwiftCorrespondentAddr}");
                c.Item().PaddingTop(4);
                SwiftRow(c, "Acc.With Institution:  57 A:", $"{SwiftAccCode}\n{SwiftAccInstitution}\n{SwiftAccAddr}");
                c.Item().PaddingTop(4);
                SwiftRow(c, "Beneficiary: 59:", $"IBAN    {SwiftBeneficiaryIban}\n{SwiftBeneficiaryName}\n{SwiftBeneficiaryAddr}");

                c.Item().PaddingTop(8).Text(text =>
                {
                    text.Span("Ukupan iznos u dinarima: ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    text.Span(iznosRsdStr).FontSize(8).Bold();
                });

                c.Item().PaddingTop(4).Text(PdvOslobodjenje).FontSize(8).Bold();
            });
        }

        private static void SwiftRow(ColumnDescriptor col, string label, string value)
        {
            col.Item().Row(r =>
            {
                r.ConstantItem(155).Text(label).FontSize(8).Bold();
                r.RelativeItem().Text(value).FontSize(8).Bold();
            });
        }

        // ═══════════════════════════════════════════════════════════════
        // DEFINICIJE KOLONA TABELE
        // ═══════════════════════════════════════════════════════════════

        private static void DefineDomaceColumns(TableDescriptor table)
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(22);   // #
                cols.RelativeColumn(4.5f); // Artikal
                cols.RelativeColumn(3.2f); // Pakovanje (+30%)
                cols.RelativeColumn(1.8f); // Količina
                cols.RelativeColumn(1.8f); // J. Cena RSD
                cols.RelativeColumn(1.8f); // Iznos RSD
            });
        }

        private static void DefineInoColumns(TableDescriptor table)
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(22);   // #
                cols.RelativeColumn(4.5f); // Artikal
                cols.RelativeColumn(3.2f); // Pakovanje (+30%)
                cols.RelativeColumn(1.8f); // Quantity
                cols.RelativeColumn(1.8f); // Unit Price EUR
                cols.RelativeColumn(1.8f); // Amount EUR
            });
        }

        // ═══════════════════════════════════════════════════════════════
        // FOOTER
        // ═══════════════════════════════════════════════════════════════

        private void BuildFooter(IContainer container, FakturaDetaljiModel model)
        {
            var footerTekst = "ODETTA DOO | Kralja Dragutina 5, 7/31, 15000 Šabac | www.odetta.rs";

            container
                .Background(Colors.Grey.Lighten3)
                .Padding(8)
                .Row(row =>
                {
                    row.RelativeItem().AlignLeft().AlignMiddle()
                        .Text(footerTekst)
                        .FontSize(7).FontColor(Colors.Grey.Darken1);

                    row.ConstantItem(100).AlignRight().AlignMiddle().Text(x =>
                    {
                        x.Span("Stranica ").FontSize(8).FontColor(Colors.Black);
                        x.CurrentPageNumber().FontSize(8).FontColor(Colors.Black);
                        x.Span(" / ").FontSize(8).FontColor(Colors.Black);
                        x.TotalPages().FontSize(8).FontColor(Colors.Black);
                    });
                });
        }

        // ═══════════════════════════════════════════════════════════════
        // HELPER METODE
        // ═══════════════════════════════════════════════════════════════

        private static void DocRow(ColumnDescriptor col, string label, string value)
        {
            col.Item().Row(r =>
            {
                r.ConstantItem(110).Text(label + ":").FontSize(8).FontColor(Colors.Grey.Darken1);
                r.RelativeItem().Text(value).FontSize(8).SemiBold();
            });
        }

        private static void HeaderCell(TableCellDescriptor header, string text,
            string align = "left")
        {
            var cell = header.Cell()
                .Background(Colors.Grey.Darken2)
                .PaddingVertical(5).PaddingHorizontal(6)
                .AlignMiddle();

            if (align == "right") cell.AlignRight().Text(text).FontSize(8).Bold().FontColor(Colors.White);
            else if (align == "center") cell.AlignCenter().Text(text).FontSize(8).Bold().FontColor(Colors.White);
            else cell.Text(text).FontSize(8).Bold().FontColor(Colors.White);
        }

        private static void DataCell(TableDescriptor table, string text, string bg,
            string align = "left")
        {
            var cell = table.Cell()
                .Background(bg)
                .BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(4).PaddingHorizontal(6)
                .AlignMiddle();

            if (align == "right")
                cell.AlignRight().Text(text).FontSize(8);
            else if (align == "center")
                cell.AlignCenter().Text(text).FontSize(8);
            else
                cell.Text(text).FontSize(8);
        }

        // Posebna ćelija za pakovanje: max 2 reda visine
        private static void DataCellPakovanje(TableDescriptor table, string text, string bg)
        {
            table.Cell()
                .Background(bg)
                .BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(4).PaddingHorizontal(6)
                .AlignMiddle()
                .MaxHeight(34) // ~2 reda teksta pri font-size 8
                .Text(text).FontSize(8).LineHeight(1.2f);
        }

        private static void TotalRow(ColumnDescriptor col, string label, string value,
            bool bold = false, string? bgColor = null)
        {
            var item = col.Item();
            if (bgColor != null) item = item.Background(bgColor);

            item.BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(3).PaddingHorizontal(6)
                .Row(r =>
                {
                    if (bold)
                    {
                        r.RelativeItem().Text(label + ":").FontSize(9).Bold().FontColor(Colors.Grey.Darken2);
                        r.ConstantItem(100).AlignRight().Text(value).FontSize(9).Bold();
                    }
                    else
                    {
                        r.RelativeItem().Text(label + ":").FontSize(9).FontColor(Colors.Grey.Darken2);
                        r.ConstantItem(100).AlignRight().Text(value).FontSize(9);
                    }
                });
        }

        private static string FormatPakovanje(FakturaStavkaModel s)
        {
            if (string.IsNullOrEmpty(s.Pakovanje)) return "-";
            return s.BrojJPuGP > 0 ? $"{s.Pakovanje} ({s.BrojJPuGP} kom)" : s.Pakovanje;
        }

        // ═══════════════════════════════════════════════════════════════
        // SLOVIMA — konverzija iznosa u tekst (srpski, RSD)
        // ═══════════════════════════════════════════════════════════════

        private static string IznosSlovima(decimal iznos)
        {
            long dinari = (long)Math.Floor(iznos);
            int pare = (int)Math.Round((iznos - dinari) * 100);

            string tekst = BrojSlovima(dinari);
            string valuta = dinari % 10 == 1 && dinari % 100 != 11 ? "dinar"
                : dinari % 10 >= 2 && dinari % 10 <= 4 && (dinari % 100 < 10 || dinari % 100 >= 20) ? "dinara"
                : "dinara";

            if (pare > 0)
                return $"{char.ToUpper(tekst[0])}{tekst[1..]} {valuta} i {pare:D2}/100";
            return $"{char.ToUpper(tekst[0])}{tekst[1..]} {valuta}";
        }

        private static string BrojSlovima(long n)
        {
            if (n == 0) return "nula";
            if (n < 0) return "minus " + BrojSlovima(-n);

            string[] jedinice = { "", "jedan", "dva", "tri", "četiri", "pet", "šest", "sedam", "osam", "devet",
                "deset", "jedanaest", "dvanaest", "trinaest", "četrnaest", "petnaest", "šesnaest",
                "sedamnaest", "osamnaest", "devetnaest" };
            string[] desetice = { "", "", "dvadeset", "trideset", "četrdeset", "pedeset",
                "šezdeset", "sedamdeset", "osamdeset", "devedeset" };

            if (n < 20) return jedinice[n];
            if (n < 100) return desetice[n / 10] + (n % 10 > 0 ? " " + jedinice[n % 10] : "");
            if (n < 1000)
            {
                string s = n / 100 == 1 ? "sto" : jedinice[n / 100] + "sto";
                return s + (n % 100 > 0 ? " " + BrojSlovima(n % 100) : "");
            }
            if (n < 1_000_000)
            {
                long h = n / 1000;
                string s = h == 1 ? "hiljadu"
                    : h == 2 ? "dve hiljade"
                    : h % 10 >= 2 && h % 10 <= 4 && (h % 100 < 10 || h % 100 >= 20) ? BrojSlovima(h) + " hiljade"
                    : BrojSlovima(h) + " hiljada";
                return s + (n % 1000 > 0 ? " " + BrojSlovima(n % 1000) : "");
            }
            if (n < 1_000_000_000)
            {
                long m = n / 1_000_000;
                string s = m == 1 ? "jedan milion"
                    : m % 10 >= 2 && m % 10 <= 4 && (m % 100 < 10 || m % 100 >= 20) ? BrojSlovima(m) + " miliona"
                    : BrojSlovima(m) + " miliona";
                return s + (n % 1_000_000 > 0 ? " " + BrojSlovima(n % 1_000_000) : "");
            }
            {
                long mil = n / 1_000_000_000;
                return BrojSlovima(mil) + " milijardi" + (n % 1_000_000_000 > 0 ? " " + BrojSlovima(n % 1_000_000_000) : "");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // PREVODI — srpski i engleski labele
        // ═══════════════════════════════════════════════════════════════

        private static readonly Labels SrLabels = new()
        {
            DocumentInfo = "Podaci dokumenta",
            BillTo = "Kupac",
            InvoiceNo = "Broj fakture",
            Date = "Datum",
            Currency = "Valuta",
            ExchangeRate = "Kurs",
            DeliveryNote = "Otpremnica",
            ContractNo = "Br. ugovora",
            DeliveryTerms = "Paritet",
            PaymentTerms = "Uslovi plaćanja",
            Vat = "PIB",
            RegNo = "MB",
            AccountNo = "Br. računa",
            Description = "Artikal",
            Packaging = "Pakovanje",
            Quantity = "Količina (kg)",
            UnitPrice = "Jed. cena",
            Amount = "Iznos",
            NetBase = "Neto osnova",
            Pdv = "PDV",
            Total = "UKUPNO",
            PaymentInfo = "Podaci za plaćanje",
            AuthorizedBy = "Overio / Pečat",
            ReceivedBy = "Primio",
            TransportInfo = "Podaci o isporuci",
            Vehicle = "Vozilo",
            Lot = "LOT",
            WorkOrder = "Radni nalog",
            NumPackages = "Broj pakovanja",
            GrossWeight = "Bruto težina",
            InWords = "Slovima",
        };

        private static readonly Labels EnLabels = new()
        {
            DocumentInfo = "Document Details",
            BillTo = "Bill To",
            InvoiceNo = "Invoice No",
            Date = "Date",
            Currency = "Currency",
            ExchangeRate = "Exchange Rate",
            DeliveryNote = "Delivery Note",
            ContractNo = "Contract No",
            DeliveryTerms = "Delivery Terms",
            PaymentTerms = "Payment Terms",
            Vat = "VAT No",
            RegNo = "Reg. No",
            AccountNo = "Bank Account",
            Description = "Description",
            Packaging = "Packaging",
            Quantity = "Quantity (kg)",
            UnitPrice = "Unit Price",
            Amount = "Amount",
            NetBase = "Net Base",
            Pdv = "VAT",
            Total = "TOTAL",
            PaymentInfo = "Instructions for Payment",
            AuthorizedBy = "Authorized by / Stamp",
            ReceivedBy = "Received by",
            TransportInfo = "Shipment Details",
            Vehicle = "LKW NR",
            Lot = "LOT",
            WorkOrder = "Work Order",
            NumPackages = "Number of Packages",
            GrossWeight = "GROSS",
            InWords = "In words",
        };

        private class Labels
        {
            public string DocumentInfo { get; init; } = "";
            public string BillTo { get; init; } = "";
            public string InvoiceNo { get; init; } = "";
            public string Date { get; init; } = "";
            public string Currency { get; init; } = "";
            public string ExchangeRate { get; init; } = "";
            public string DeliveryNote { get; init; } = "";
            public string ContractNo { get; init; } = "";
            public string DeliveryTerms { get; init; } = "";
            public string PaymentTerms { get; init; } = "";
            public string Vat { get; init; } = "";
            public string RegNo { get; init; } = "";
            public string AccountNo { get; init; } = "";
            public string Description { get; init; } = "";
            public string Packaging { get; init; } = "";
            public string Quantity { get; init; } = "";
            public string UnitPrice { get; init; } = "";
            public string Amount { get; init; } = "";
            public string NetBase { get; init; } = "";
            public string Pdv { get; init; } = "";
            public string Total { get; init; } = "";
            public string PaymentInfo { get; init; } = "";
            public string AuthorizedBy { get; init; } = "";
            public string ReceivedBy { get; init; } = "";
            public string TransportInfo { get; init; } = "";
            public string Vehicle { get; init; } = "";
            public string Lot { get; init; } = "";
            public string WorkOrder { get; init; } = "";
            public string NumPackages { get; init; } = "";
            public string GrossWeight { get; init; } = "";
            public string InWords { get; init; } = "";
        }
    }
}
