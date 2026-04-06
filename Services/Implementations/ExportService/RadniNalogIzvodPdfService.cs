using FruitSysWeb.Models.IzvodDokumenata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    /// <summary>
    /// Generiše PDF za Radni Nalog sa svim kontrolama (Proces, Temperatura, Težina, Završna).
    /// </summary>
    public class RadniNalogIzvodPdfService
    {
        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        private static readonly string LogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo.png");

        private const string OdettaNaziv = "ODETTA DOO";
        private const string OdettaAdresa = "Kralja Dragutina 5, 7/31, 15000 Šabac, Srbija";
        private const string OdettaPib = "102679301";
        private const string OdettaMb = "17392344";

        public byte[] Generisi(IzvodRadniNalogDetalji model)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily(Fonts.Arial));

                    page.Header().Element(c => BuildHeader(c, model));
                    page.Content().Element(c => BuildContent(c, model));
                    page.Footer().Element(BuildFooter);
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        // ── HEADER ─────────────────────────────────────────────────────────

        private void BuildHeader(IContainer container, IzvodRadniNalogDetalji model)
        {
            container.Column(col =>
            {
                col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(8).Row(row =>
                {
                    if (File.Exists(LogoPath))
                        row.ConstantItem(80).PaddingRight(10).AlignMiddle().Image(LogoPath).FitArea();

                    row.RelativeItem().AlignMiddle().Column(c =>
                    {
                        c.Item().Text($"{OdettaNaziv}, {OdettaAdresa}").FontSize(9).Bold();
                        c.Item().Text($"PIB: {OdettaPib}  |  MB: {OdettaMb}").FontSize(7).FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(200).AlignMiddle().AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text($"RADNI NALOG: {model.Sifra}").FontSize(13).Bold();
                        c.Item().AlignRight().Text($"Komitent: {model.Komitent}").FontSize(9).SemiBold()
                            .FontColor(Colors.Grey.Darken1);
                    });
                });

                col.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        InfoRow(c, "Artikal:", model.Artikal);
                        InfoRow(c, "Pakovanje:", model.Pakovanje);
                        InfoRow(c, "Količina:", model.KolicinaFormatted + " kg");
                    });
                    row.RelativeItem().Column(c =>
                    {
                        InfoRow(c, "Datum početka:", model.DatumPocetkaFormatted);
                        InfoRow(c, "Datum isporuke:", model.DatumIsporukeFormatted);
                        if (model.BrojPakovanja > 0)
                            InfoRow(c, "Br. pakovanja:", model.BrojPakovanja.ToString());
                    });
                    row.RelativeItem().Column(c =>
                    {
                        if (!string.IsNullOrEmpty(model.LotNaloga))
                            InfoRow(c, "LOT:", model.LotNaloga);
                        if (!string.IsNullOrEmpty(model.Ugovor))
                            InfoRow(c, "Ugovor:", model.Ugovor);
                        if (!string.IsNullOrEmpty(model.Opis))
                            InfoRow(c, "Opis:", model.Opis);
                    });
                });

                col.Item().PaddingBottom(6);
            });
        }

        // ── CONTENT ────────────────────────────────────────────────────────

        private void BuildContent(IContainer container, IzvodRadniNalogDetalji model)
        {
            container.Column(col =>
            {
                // 1. Kontrole u procesu proizvodnje
                if (model.KontroleProzvodnje.Count > 0)
                {
                    col.Item().Text("Kontrole u procesu proizvodnje").FontSize(9).Bold()
                        .FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(2).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(80);  // Datum
                            c.ConstantColumn(45);  // Smena
                            c.RelativeColumn(2);    // Artikal
                            c.RelativeColumn(1.5f); // Pakovanje
                            c.RelativeColumn(1);    // Kolicina
                            c.RelativeColumn(2);    // Mesto Kontrole
                            c.RelativeColumn(1);    // Rezultat
                            c.RelativeColumn(2);    // Pregledao
                        });

                        table.Header(h =>
                        {
                            HeaderCell(h, "Datum");
                            HeaderCell(h, "Smena");
                            HeaderCell(h, "Artikal");
                            HeaderCell(h, "Pakovanje");
                            HeaderCell(h, "Količina");
                            HeaderCell(h, "Mesto Kontrole");
                            HeaderCell(h, "Rezultat");
                            HeaderCell(h, "Pregledao");
                        });

                        foreach (var k in model.KontroleProzvodnje)
                        {
                            var bg = k.RezultatKontrole == 1 ? Colors.Green.Lighten5 : Colors.Red.Lighten5;
                            DataCell(table, k.DatumFormatted, false, bg);
                            DataCell(table, k.Smena.ToString(), true, bg);
                            DataCell(table, k.Artikal, false, bg);
                            DataCell(table, k.Pakovanje, false, bg);
                            DataCell(table, k.Kolicina.ToString("N2", Sr), true, bg);
                            DataCell(table, k.MestoKontrole ?? "-", false, bg);
                            table.Cell().Background(bg).Border(0.3f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(3).AlignCenter()
                                .Text(k.RezultatTekst).FontSize(8).Bold()
                                .FontColor(k.RezultatKontrole == 1 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            DataCell(table, k.Pregledao ?? "-", false, bg);
                        }
                    });
                    col.Item().PaddingVertical(6);
                }

                // 2. Kontrola temperature
                if (model.Temperature.Count > 0)
                {
                    col.Item().Text("Kontrola temperature").FontSize(9).Bold()
                        .FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(2).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(75);  // Datum
                            c.ConstantColumn(40);  // Smena
                            c.RelativeColumn(1.5f); // Artikal
                            c.RelativeColumn(1);    // LOT
                            c.RelativeColumn(1);    // Vrsta pak.
                            c.RelativeColumn(3);    // Merenja (tekstualno)
                            c.RelativeColumn(1.5f); // Pregledao
                        });

                        table.Header(h =>
                        {
                            HeaderCell(h, "Datum");
                            HeaderCell(h, "Smena");
                            HeaderCell(h, "Artikal");
                            HeaderCell(h, "LOT");
                            HeaderCell(h, "Pakovanje");
                            HeaderCell(h, "Merenja (°C)");
                            HeaderCell(h, "Pregledao");
                        });

                        foreach (var t in model.Temperature)
                        {
                            DataCell(table, t.DatumFormatted, false, Colors.White);
                            DataCell(table, t.Smena.ToString(), true, Colors.White);
                            DataCell(table, t.Artikal ?? "-", false, Colors.White);
                            DataCell(table, t.BrojLota ?? "-", false, Colors.White);
                            DataCell(table, t.VrstaPakovanja ?? "-", false, Colors.White);

                            var merenja = t.GetMerenja();
                            var mText = merenja.Count > 0
                                ? string.Join("  ", merenja.Select(m => m.Vrednost))
                                : "-";
                            DataCell(table, mText, false, Colors.White);
                            DataCell(table, t.Pregledao ?? "-", false, Colors.White);
                        }
                    });
                    col.Item().PaddingVertical(6);
                }

                // 3. Kontrola težine
                if (model.Tezine.Count > 0)
                {
                    col.Item().Text("Kontrola težine").FontSize(9).Bold()
                        .FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(2).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(75);
                            c.ConstantColumn(40);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);    // Vaga
                            c.RelativeColumn(3);    // Merenja
                            c.RelativeColumn(1.5f);
                        });

                        table.Header(h =>
                        {
                            HeaderCell(h, "Datum");
                            HeaderCell(h, "Smena");
                            HeaderCell(h, "Artikal");
                            HeaderCell(h, "LOT");
                            HeaderCell(h, "Pakovanje");
                            HeaderCell(h, "Vaga");
                            HeaderCell(h, "Merenja (g)");
                            HeaderCell(h, "Pregledao");
                        });

                        foreach (var t in model.Tezine)
                        {
                            DataCell(table, t.DatumFormatted, false, Colors.White);
                            DataCell(table, t.Smena.ToString(), true, Colors.White);
                            DataCell(table, t.Artikal ?? "-", false, Colors.White);
                            DataCell(table, t.BrojLota ?? "-", false, Colors.White);
                            DataCell(table, t.VrstaPakovanja ?? "-", false, Colors.White);
                            DataCell(table, t.Vaga ?? "-", false, Colors.White);

                            var merenja = t.GetMerenja();
                            var mText = merenja.Count > 0
                                ? string.Join("  ", merenja.Select(m => m.Vrednost))
                                : "-";
                            DataCell(table, mText, false, Colors.White);
                            DataCell(table, t.Pregledao ?? "-", false, Colors.White);
                        }
                    });
                    col.Item().PaddingVertical(6);
                }

                // 4. Završna kontrola
                if (model.ZavrsneKontrole.Count > 0)
                {
                    col.Item().Text("Završna kontrola").FontSize(9).Bold()
                        .FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(2).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(80);
                            c.ConstantColumn(45);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1.5f); // Vozilo
                        });

                        table.Header(h =>
                        {
                            HeaderCell(h, "Datum");
                            HeaderCell(h, "Smena");
                            HeaderCell(h, "Artikal");
                            HeaderCell(h, "Pakovanje");
                            HeaderCell(h, "Količina");
                            HeaderCell(h, "Mesto Kontrole");
                            HeaderCell(h, "Rezultat");
                            HeaderCell(h, "Pregledao");
                            HeaderCell(h, "Vozilo");
                        });

                        foreach (var z in model.ZavrsneKontrole)
                        {
                            var bg = z.RezultatKontrole == 1 ? Colors.Green.Lighten5 : Colors.Red.Lighten5;
                            DataCell(table, z.DatumFormatted, false, bg);
                            DataCell(table, z.Smena.ToString(), true, bg);
                            DataCell(table, z.Artikal, false, bg);
                            DataCell(table, z.Pakovanje, false, bg);
                            DataCell(table, z.Kolicina.ToString("N2", Sr), true, bg);
                            DataCell(table, z.MestoKontrole ?? "-", false, bg);
                            table.Cell().Background(bg).Border(0.3f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(3).AlignCenter()
                                .Text(z.RezultatTekst).FontSize(8).Bold()
                                .FontColor(z.RezultatKontrole == 1 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            DataCell(table, z.Pregledao ?? "-", false, bg);
                            DataCell(table, z.Vozilo ?? "-", false, bg);
                        }
                    });
                    col.Item().PaddingVertical(4);
                }

                if (model.KontroleProzvodnje.Count == 0 && model.Temperature.Count == 0
                    && model.Tezine.Count == 0 && model.ZavrsneKontrole.Count == 0)
                {
                    col.Item().PaddingTop(20).AlignCenter()
                        .Text("Nema evidentiranih kontrola za ovaj radni nalog.")
                        .FontSize(10).FontColor(Colors.Grey.Darken1).Italic();
                }
            });
        }

        // ── FOOTER ─────────────────────────────────────────────────────────

        private void BuildFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(3)
                    .Text($"{OdettaNaziv}  |  {OdettaAdresa}").FontSize(7).FontColor(Colors.Grey.Darken1);
                row.ConstantItem(100).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(3)
                    .AlignRight().Text(ctx =>
                    {
                        ctx.Span("Strana ").FontSize(7).FontColor(Colors.Grey.Darken1);
                        ctx.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Darken1);
                    });
            });
        }

        // ── HELPERS ────────────────────────────────────────────────────────

        private static void HeaderCell(TableCellDescriptor header, string text)
        {
            header.Cell()
                .Background(Colors.Blue.Darken3).BorderBottom(0.5f).BorderColor(Colors.Grey.Darken1)
                .Padding(3).AlignCenter()
                .Text(text).FontSize(7.5f).Bold().FontColor(Colors.White);
        }

        private static void DataCell(TableDescriptor table, string text, bool alignRight, string bg)
        {
            var cell = table.Cell().Background(bg).BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).Padding(2);
            if (alignRight)
                cell.AlignRight().Text(text).FontSize(8);
            else
                cell.Text(text).FontSize(8);
        }

        private static void InfoRow(ColumnDescriptor col, string label, string value)
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(100).Text(label).FontSize(8).FontColor(Colors.Grey.Darken2);
                row.RelativeItem().Text(value).FontSize(8);
            });
        }
    }
}
