using FruitSysWeb.Models.IzvodDokumenata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    public class PrijemnicaPdfService
    {
        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        private static readonly string LogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo.png");

        private const string OdettaNaziv = "ODETTA DOO, Kralja Dragutina 5, 7/31, 15000 Šabac, Srbija";
        private const string OdettaPib = "102679301";
        private const string OdettaMb = "17392344";
        private const string OdettaBanka = "AIK Banka 105-14819-95";
        private const string Pregledao = "Jelena Apt";

        public byte[] Generisi(IzvodPrijemnicaDetalji model)
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

        private void BuildHeader(IContainer container, IzvodPrijemnicaDetalji model)
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

                // ── Datum + Naslov ──
                col.Item().PaddingTop(6).PaddingBottom(4).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Row(row =>
                {
                    row.ConstantItem(80).Column(c =>
                    {
                        c.Item().Text("Datum:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        c.Item().Text(model.DatumFormatted).FontSize(9).Bold();
                    });
                    row.RelativeItem().AlignCenter().Text($"PRIJEMNICA - BR:  {model.Sifra}").FontSize(14).Bold();
                });

                col.Item().PaddingBottom(4);

                // ── Komitent + Adresa ──
                col.Item().BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(65).Text("Komitent:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.RelativeItem().Text(model.Komitent).FontSize(9).SemiBold();
                    if (!string.IsNullOrEmpty(model.KomitentAdresa))
                    {
                        row.ConstantItem(45).Text("Adresa:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.RelativeItem().Text(model.KomitentAdresa).FontSize(8);
                    }
                });

                // ── Vozac + Vozilo + Ugovor ──
                col.Item().PaddingTop(3).BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(40).Text("Vozac:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.ConstantItem(130).Text(model.Vozac).FontSize(8);
                    row.ConstantItem(35).Text("Vozilo:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.ConstantItem(110).Text(model.Vozilo).FontSize(8);
                    row.ConstantItem(40).Text("Ugovor:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.RelativeItem().Text(string.Empty).FontSize(8);
                });

                // ── Roba label + Otpremnica + Otkupno mesto ──
                col.Item().PaddingTop(3).PaddingBottom(6).Row(row =>
                {
                    row.ConstantItem(35).Text("Roba:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    if (!string.IsNullOrEmpty(model.OtpremnicaDobavljaca))
                    {
                        row.ConstantItem(60).Text("Otpremnica:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.ConstantItem(80).Text(model.OtpremnicaDobavljaca).FontSize(8);
                    }
                    else
                    {
                        row.ConstantItem(140).Text(string.Empty);
                    }
                    row.ConstantItem(70).Text("Otkupno mesto:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.RelativeItem().Text(string.Empty).FontSize(8);
                });
            });
        }

        private void BuildContent(IContainer container, IzvodPrijemnicaDetalji model)
        {
            container.Column(col =>
            {
                // ── Tabela Roba ──
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(28);   // RB
                        c.RelativeColumn(5);     // Artikal
                        c.RelativeColumn(1);     // JM
                        c.RelativeColumn(1.5f);  // Kolicina
                        c.RelativeColumn(1.5f);  // Rok
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

                col.Item().PaddingVertical(6);

                // ── Specifikacija ──
                col.Item().Text("Specifikacija:").FontSize(9);
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
                        HeaderCell(h, "KOLICINA");
                        HeaderCell(h, "%");
                    });

                    int rb = 1;
                    foreach (var stavka in model.Specifikacija)
                    {
                        DataCellCenter(table, rb.ToString());
                        DataCellLeft(table, stavka.Artikal);
                        DataCellCenter(table, stavka.JM);
                        DataCellRight(table, stavka.KolicinaFormatted);
                        DataCellRight(table, stavka.ProcenatFormatted);
                        rb++;
                    }
                });

                col.Item().PaddingVertical(8);

                // ── KVVK ──
                col.Item().Text("KVALITATIVNO VIZUELNO KONTROLISANJE").FontSize(9).Bold();
                col.Item().PaddingTop(4).Column(kv =>
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
                    kv.Item().PaddingBottom(4).Row(row =>
                    {
                        row.ConstantItem(130).Text("Temperatura primljene robe:").FontSize(8);
                        row.ConstantItem(40).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                            .Text(model.Temperatura.HasValue ? $"{model.Temperatura:N0}" : string.Empty).FontSize(8);
                        row.ConstantItem(16).AlignCenter().Text("°C").FontSize(8);
                        row.ConstantItem(50).Text("Primedba:").FontSize(8);
                        row.RelativeItem().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                            .Text(model.TemperaturaPrimedba ?? string.Empty).FontSize(8);
                    });

                    // Red 3: Rezultat kvant.-viz.
                    kv.Item().PaddingBottom(4).Row(row =>
                    {
                        row.ConstantItem(240).Text("Rezultat kvantitativnog – vizuelnog kontrolisanja:").FontSize(8);
                        CheckBox(row, "OK", model.VizuelnaKontrola == true);
                        row.ConstantItem(6);
                        CheckBox(row, "NOK", model.VizuelnaKontrola == false);
                        row.RelativeItem().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Text(
                            model.VizuelnaKontrolaPrimedba ?? string.Empty).FontSize(8);
                    });

                    // Red 4: Stanje Robe/Pakovanja
                    kv.Item().PaddingBottom(4).Row(row =>
                    {
                        row.ConstantItem(130).Text("Stanje Robe/Pakovanja:").FontSize(8);
                        CheckBox(row, "OK", model.StanjeRobePakovanja == true);
                        row.ConstantItem(6);
                        CheckBox(row, "NOK", model.StanjeRobePakovanja == false);
                        row.RelativeItem().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                            .Text(model.StanjeRobePakovanjaPrimedba ?? string.Empty).FontSize(8);
                    });
                });

                col.Item().PaddingVertical(6);

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

                col.Item().PaddingVertical(8);

                // ── Postupak sa neusaglašenim ──
                col.Item().Text("POSTUPAK SA NEUSAGLAŠENIM PROIZVODOM").FontSize(9).Bold();
                col.Item().PaddingTop(4).Column(ns =>
                {
                    ns.Item().PaddingBottom(4).Row(row =>
                    {
                        row.ConstantItem(110).Text("Neusaglašeni proizvod:").FontSize(8);
                        row.ConstantItem(60).Text(CheckLabel(model.Primiti) + "  1.PRIMITI").FontSize(8);
                        row.ConstantItem(70).Text(CheckLabel(model.Vratiti) + "  2.VRATITI").FontSize(8);
                        row.RelativeItem().Text(CheckLabel(model.Reklamirati) + "  3.REKLAMIRATI").FontSize(8);
                    });
                    ns.Item().PaddingBottom(4).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Row(row =>
                    {
                        row.ConstantItem(100).Text("PRIMITI UZ SELEKCIJU").FontSize(8);
                        row.RelativeItem().Text(model.PrimitiUzSelekciju == true ? "DA" : string.Empty).FontSize(8);
                    });
                    ns.Item().PaddingBottom(4).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Row(row =>
                    {
                        row.ConstantItem(100).Text("USLOVNO PRIMITI ZA").FontSize(8);
                        row.RelativeItem().Text(model.UslovnoPrimitiZa ?? string.Empty).FontSize(8);
                    });
                });

                col.Item().PaddingVertical(10);

                // ── Overio i odobrio ──
                col.Item().AlignRight().Text("Overio i odobrio:").FontSize(8).FontColor(Colors.Grey.Darken2);
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

        private static string CheckLabel(bool? value) => value == true ? "[X]" : "[ ]";
    }
}
