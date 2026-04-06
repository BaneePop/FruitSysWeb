using FruitSysWeb.Models.IzvodDokumenata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    public class OtpremnicaPdfService
    {
        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        private static readonly string LogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo.png");

        private const string OdettaNaziv = "ODETTA DOO, Kralja Dragutina 5, 7/31, 15000 Šabac, Srbija";
        private const string OdettaPib = "102679301";
        private const string OdettaMb = "17392344";
        private const string OdettaBanka = "AIK Banka 105-14819-95";
        private const string Pregledao = "Jelena Apt";

        public byte[] Generisi(IzvodOtpremnicaDetalji model)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                    page.Header().Element(c => BuildHeader(c, model));
                    page.Content().Element(c => BuildContent(c, model));
                    page.Footer().Element(BuildFooter);
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        private void BuildHeader(IContainer container, IzvodOtpremnicaDetalji model)
        {
            container.Column(col =>
            {
                // ── Logo + Kompanija ──
                col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(6).Row(row =>
                {
                    if (File.Exists(LogoPath))
                        row.ConstantItem(90).PaddingRight(10).AlignMiddle().Image(LogoPath).FitArea();

                    row.RelativeItem().AlignMiddle().Column(c =>
                    {
                        c.Item().Text("Društvo za trgovinu i usluge").FontSize(8).FontColor(Colors.Grey.Darken2);
                        c.Item().Text(OdettaNaziv).FontSize(9).Bold();
                        c.Item().PaddingTop(2).Row(r =>
                        {
                            r.RelativeItem().Text(t =>
                            {
                                t.Span("PIB:").FontSize(8).FontColor(Colors.Grey.Darken2);
                                t.Span($"          {OdettaPib}").FontSize(8);
                            });
                            r.RelativeItem().AlignRight().Text(OdettaBanka).FontSize(8).FontColor(Colors.Grey.Darken2);
                        });
                        c.Item().Text(t =>
                        {
                            t.Span("Matični broj:").FontSize(8).FontColor(Colors.Grey.Darken2);
                            t.Span($"  {OdettaMb}").FontSize(8);
                        });
                    });
                });

                // ── Datum + Naslov + RN ──
                col.Item().PaddingTop(6).PaddingBottom(4).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Row(row =>
                {
                    row.ConstantItem(80).Column(c =>
                    {
                        c.Item().Text("Datum:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        c.Item().Text(model.DatumFormatted).FontSize(9).Bold();
                    });
                    row.RelativeItem().AlignCenter().Text($"OTPREMNICA - BR:  {model.Sifra}").FontSize(13).Bold();
                    if (!string.IsNullOrEmpty(model.RadniNalog))
                        row.ConstantItem(100).AlignRight().AlignMiddle().Text(model.RadniNalog).FontSize(11).Bold();
                });

                col.Item().PaddingTop(5);

                // ── Komitent ──
                col.Item().BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(60).Text("Komitent:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.RelativeItem().Text(model.Komitent).FontSize(9).SemiBold();
                    if (!string.IsNullOrEmpty(model.KomitentPib))
                    {
                        row.ConstantItem(28).Text("PIB:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.ConstantItem(80).Text(model.KomitentPib).FontSize(8);
                    }
                });

                // ── Komitent Adresa + MB ──
                if (!string.IsNullOrEmpty(model.KomitentAdresa))
                {
                    col.Item().BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(3).Row(row =>
                    {
                        row.ConstantItem(60).Text("Adresa:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.RelativeItem().Text(model.KomitentAdresa).FontSize(8);
                        if (!string.IsNullOrEmpty(model.KomitentMb))
                        {
                            row.ConstantItem(28).Text("MB:").FontSize(8).FontColor(Colors.Grey.Darken2);
                            row.ConstantItem(80).Text(model.KomitentMb).FontSize(8);
                        }
                    });
                }

                // ── Vozar ──
                if (!string.IsNullOrEmpty(model.Vozar))
                {
                    col.Item().BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(3).Row(row =>
                    {
                        row.ConstantItem(40).Text("Vozar:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.RelativeItem().Text(model.Vozar).FontSize(9).SemiBold();
                        if (!string.IsNullOrEmpty(model.VozarPib))
                        {
                            row.ConstantItem(28).Text("PIB:").FontSize(8).FontColor(Colors.Grey.Darken2);
                            row.ConstantItem(80).Text(model.VozarPib).FontSize(8);
                        }
                    });

                    // Vozar Adresa + MB
                    if (!string.IsNullOrEmpty(model.VozarAdresa))
                    {
                        col.Item().BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(3).Row(row =>
                        {
                            row.ConstantItem(40).Text("Adresa:").FontSize(8).FontColor(Colors.Grey.Darken2);
                            row.RelativeItem().Text(model.VozarAdresa).FontSize(8);
                        });
                    }
                }

                // ── Vozac + Br. Pasoša + Vozilo ──
                col.Item().BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(40).Text("Vozac:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.ConstantItem(110).Text(model.Vozac).FontSize(8);
                    if (!string.IsNullOrEmpty(model.BrojPasosa))
                    {
                        row.ConstantItem(60).Text("Br. pasoša:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.ConstantItem(80).Text(model.BrojPasosa).FontSize(8);
                    }
                    else
                    {
                        row.ConstantItem(140);
                    }
                    row.ConstantItem(35).Text("Vozilo:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.RelativeItem().Text(model.Vozilo).FontSize(8);
                });

                // ── Roba: + Granični prelaz + Lot ──
                col.Item().PaddingTop(3).PaddingBottom(6).Row(row =>
                {
                    row.ConstantItem(35).Text("Roba:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    if (!string.IsNullOrEmpty(model.GranicniPrelaz))
                    {
                        row.ConstantItem(80).Text("Granicni prelaz:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.ConstantItem(80).Text(model.GranicniPrelaz).FontSize(8).Bold();
                    }
                    else
                    {
                        row.ConstantItem(160);
                    }
                    if (!string.IsNullOrEmpty(model.Lot))
                    {
                        row.ConstantItem(28).Text("Lot:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.RelativeItem().Text(model.Lot).FontSize(8);
                    }
                });
            });
        }

        private void BuildContent(IContainer container, IzvodOtpremnicaDetalji model)
        {
            container.Column(col =>
            {
                // ── Tabela Roba ──
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(28);
                        c.RelativeColumn(5);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1.5f);
                        c.RelativeColumn(1.5f);
                    });

                    table.Header(h =>
                    {
                        HeaderCell(h, "RB");
                        HeaderCell(h, "NAZIV ARTIKLA");
                        HeaderCell(h, "JM");
                        HeaderCell(h, "KOLICINA");
                        HeaderCell(h, "ROK");
                    });

                    int rb = 1;
                    foreach (var stavka in model.Stavke)
                    {
                        DataCellCenter(table, rb.ToString());
                        DataCellLeft(table, stavka.Artikal);
                        DataCellCenter(table, stavka.JM);
                        DataCellRight(table, stavka.KolicinaFormatted);
                        DataCellRight(table, stavka.Rok ?? string.Empty);
                        rb++;
                    }
                });

                // ── Ambalaza ──
                if (model.Ambalaza.Count > 0)
                {
                    col.Item().PaddingTop(8).Text("Ambalaza:").FontSize(9);
                    col.Item().PaddingTop(2).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(28);
                            c.RelativeColumn(5);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1.5f);
                        });

                        table.Header(h =>
                        {
                            HeaderCell(h, "RB");
                            HeaderCell(h, "NAZIV ARTIKLA");
                            HeaderCell(h, "JM");
                            HeaderCell(h, "KOL");
                            HeaderCell(h, "TEŽINA");
                        });

                        int rb = 1;
                        decimal ukupno = 0;
                        foreach (var amb in model.Ambalaza)
                        {
                            ukupno += amb.Tezina;
                            DataCellCenter(table, rb.ToString());
                            DataCellLeft(table, amb.Artikal);
                            DataCellCenter(table, amb.JM);
                            DataCellRight(table, amb.KolicinaFormatted);
                            DataCellRight(table, amb.TezinaFormatted);
                            rb++;
                        }

                        table.Cell().ColumnSpan(4).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(3).AlignRight().Text("UKUPNO:").FontSize(8).Bold();
                        table.Cell().BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(3).AlignRight().Text(ukupno.ToString("N2", Sr)).FontSize(8);
                    });
                }

                // ── Temperatura + Bruto Težina ──
                col.Item().PaddingTop(6).Row(row =>
                {
                    if (model.Temperatura.HasValue)
                        row.RelativeItem().Text($"Temperatura robe:   {model.Temperatura:N0}   °C").FontSize(8);
                    else
                        row.RelativeItem().Text(string.Empty);
                    row.RelativeItem().AlignRight().Text($"BRUTO TEŽINA:  {model.BrutoTezinaFormatted} Kg").FontSize(9).Bold();
                });

                col.Item().PaddingVertical(4);

                // ── Napomena ──
                if (!string.IsNullOrEmpty(model.Napomena))
                {
                    col.Item().Text($"Napomena:  {model.Napomena}").FontSize(8).FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingBottom(4);
                }
                else
                {
                    col.Item().Text("Napomena:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingBottom(4);
                }

                // ── Špedicija + Broj Plombi ──
                if (!string.IsNullOrEmpty(model.Spedicija) || !string.IsNullOrEmpty(model.BrojPlombi))
                {
                    col.Item().PaddingBottom(6).Row(row =>
                    {
                        if (!string.IsNullOrEmpty(model.Spedicija))
                        {
                            row.ConstantItem(55).Text("Špedicija:").FontSize(8).FontColor(Colors.Grey.Darken2);
                            row.RelativeItem().Text(model.Spedicija).FontSize(8).Bold();
                        }
                        else
                        {
                            row.RelativeItem().Text(string.Empty);
                        }
                        if (!string.IsNullOrEmpty(model.BrojPlombi))
                        {
                            row.ConstantItem(80).Text("BROJ PLOMBI:").FontSize(8).Bold();
                            row.ConstantItem(30).Text(model.BrojPlombi).FontSize(8).Bold();
                        }
                    });
                }

                col.Item().PaddingVertical(4);

                // ── KVVK ──
                col.Item().Text("KVALITATIVNO VIZUELNO KONTROLISANJE").FontSize(9).Bold();
                col.Item().PaddingTop(5).Column(kv =>
                {
                    // Red 1: Uzorkovano DA | NE | Količina: ___ | Oznaka: ___
                    kv.Item().PaddingBottom(4).Row(row =>
                    {
                        row.ConstantItem(70).Text("Uzorkovano:").FontSize(8);
                        CheckBox(row, "DA", model.Uzorkovano == true);
                        row.ConstantItem(6);
                        CheckBox(row, "NE", model.Uzorkovano == false);
                        row.ConstantItem(16);
                        row.ConstantItem(50).Text("Količina:").FontSize(8);
                        row.ConstantItem(100).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Text(string.Empty);
                        row.ConstantItem(16);
                        row.ConstantItem(40).Text("Oznaka:").FontSize(8);
                        row.RelativeItem().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Text(string.Empty);
                    });

                    // Red 2: Temperatura + Primedba
                    if (model.Temperatura.HasValue)
                    {
                        kv.Item().PaddingBottom(4).Row(row =>
                        {
                            row.ConstantItem(100).Text("Temperatura robe:").FontSize(8);
                            row.ConstantItem(30).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                                .Text($"{model.Temperatura:N0}").FontSize(8);
                            row.ConstantItem(16).AlignCenter().Text("°C").FontSize(8);
                            row.ConstantItem(50).Text("Primedba:").FontSize(8);
                            row.RelativeItem().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                                .Text(model.TemperaturaPrimedba ?? string.Empty).FontSize(8);
                        });
                    }

                    // Red 3: Kontrola deklaracije
                    kv.Item().PaddingBottom(4).Row(row =>
                    {
                        row.ConstantItem(230).Text("Kontrola deklaracije – podataka na deklaraciji:").FontSize(8);
                        CheckBox(row, "OK", model.KontrolaDeklaracija == true);
                        row.ConstantItem(6);
                        CheckBox(row, "NOK", model.KontrolaDeklaracija == false);
                        row.RelativeItem().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Text(string.Empty);
                    });

                    // Red 4: Rezultat kvant.-viz.
                    kv.Item().PaddingBottom(4).Row(row =>
                    {
                        row.ConstantItem(230).Text("Rezultat kvantitativnog – vizuelnog kontrolisanja:").FontSize(8);
                        CheckBox(row, "OK", model.VizuelnaKontrola == true);
                        row.ConstantItem(6);
                        CheckBox(row, "NOK", model.VizuelnaKontrola == false);
                        row.RelativeItem().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                            .Text(model.VizuelnaKontrolaPrimedba ?? string.Empty).FontSize(8);
                    });

                    // Red 5: Stanje Robe/Pakovanja
                    kv.Item().PaddingBottom(4).Row(row =>
                    {
                        row.ConstantItem(130).Text("Stanje Robe/Pakovanja:").FontSize(8);
                        CheckBox(row, "OK", model.StanjeRobePakovanja == true);
                        row.ConstantItem(6);
                        CheckBox(row, "NOK", model.StanjeRobePakovanja == false);
                        row.RelativeItem().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Text(string.Empty);
                    });

                    // Red 6: Stanje Vozila
                    kv.Item().PaddingBottom(4).Row(row =>
                    {
                        row.ConstantItem(130).Text("Stanje Vozila:").FontSize(8);
                        CheckBox(row, "OK", model.StanjeVozila == true);
                        row.ConstantItem(6);
                        CheckBox(row, "NOK", model.StanjeVozila == false);
                        row.RelativeItem().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Text(string.Empty);
                    });
                });

                col.Item().PaddingVertical(8);

                // ── Potpisi ──
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(2).Text("Komitent:").FontSize(8);
                    });
                    row.ConstantItem(20);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(2).Text("Magacioner:").FontSize(8);
                    });
                    row.ConstantItem(20);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(2).Text($"Pregledao:  {Pregledao}").FontSize(8);
                    });
                });
            });
        }

        private void BuildFooter(IContainer container)
        {
            container.BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(4)
                .Text("Napomena: Sva PVC Ambalaža mora biti vraćena najkasnije do kraja 9 meseca tekuće godine. " +
                      "Za ambalažu koja ne bude vraćena do tog datuma dobavljač ce biti zadužen 150 din/ kom za PVC 4/1 i 6/1 i 300 din/kom za PVC 12/1.")
                .FontSize(7).FontColor(Colors.Grey.Darken1);
        }

        private static void HeaderCell(TableCellDescriptor h, string text)
        {
            h.Cell().Background(Colors.Grey.Darken2).Padding(4).AlignCenter()
                .Text(text).FontSize(8).Bold().FontColor(Colors.White);
        }

        private static void DataCellLeft(TableDescriptor t, string text)
        {
            t.Cell().BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(text).FontSize(8);
        }

        private static void DataCellCenter(TableDescriptor t, string text)
        {
            t.Cell().BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().Text(text).FontSize(8);
        }

        private static void DataCellRight(TableDescriptor t, string text)
        {
            t.Cell().BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text(text).FontSize(8);
        }

        private static void CheckBox(RowDescriptor row, string label, bool filled)
        {
            row.ConstantItem(26).Row(r =>
            {
                r.ConstantItem(14).Border(0.5f).BorderColor(Colors.Grey.Darken1).Padding(1)
                    .AlignCenter().AlignMiddle()
                    .Text(filled ? "X" : " ").FontSize(7).Bold();
                r.ConstantItem(2);
                r.ConstantItem(30).AlignMiddle().Text(label).FontSize(8);
            });
        }
    }
}
