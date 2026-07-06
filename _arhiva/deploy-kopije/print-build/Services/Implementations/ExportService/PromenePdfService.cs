using FruitSysWeb.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    public class PromenePdfService
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        public byte[] Generisi(PromenPeriodData data, string naslovPerioda, string periodOpis)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                    page.Header().Element(c => BuildHeader(c, naslovPerioda, periodOpis));
                    page.Content().Element(c => BuildContent(c, data));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("ODETTA DOO • Izveštaj Promene • ");
                        x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm", SrFormat)).FontColor(Colors.Grey.Medium);
                        x.Span(" • Strana ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        private void BuildHeader(IContainer container, string naslovPerioda, string periodOpis)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(inner =>
                    {
                        inner.Item().Text($"PROMENE — {naslovPerioda.ToUpper()}")
                            .FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                        inner.Item().Text($"Period: {periodOpis}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                    row.ConstantItem(120).AlignRight().Column(inner =>
                    {
                        inner.Item().Text("ODETTA DOO").FontSize(10).Bold();
                        inner.Item().Text("PIB: 106784736").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
                col.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken3);
            });
        }

        private void BuildContent(IContainer container, PromenPeriodData data)
        {
            container.Column(col =>
            {
                col.Spacing(12);

                // ── BLOK ULAZ ────────────────────────────────────
                col.Item().Element(c => BuildBlokNaslov(c, "ULAZ", Colors.Blue.Darken2));
                col.Item().Column(inner =>
                {
                    inner.Spacing(8);
                    inner.Item().Element(c => BuildUlazTabela(c, "Sirovine i Proizvodi",
                        data.UlazSirovineProizvodi, Colors.Blue.Lighten4));
                    inner.Item().Element(c => BuildUlazTabela(c, "Ambalaza (nepovratna)",
                        data.UlazAmbalaza, Colors.Blue.Lighten4));
                    inner.Item().Element(c => BuildUlazTabela(c, "Repromaterijal",
                        data.UlazRepromaterijal, Colors.Blue.Lighten4));
                });

                var ukupnoUlaz = data.UlazSirovineProizvodi.Sum(x => x.Vrednost)
                    + data.UlazAmbalaza.Sum(x => x.Vrednost)
                    + data.UlazRepromaterijal.Sum(x => x.Vrednost);
                col.Item().Element(c => BuildUkupnoRed(c, "UKUPNO ULAZ", ukupnoUlaz, Colors.Blue.Darken2));

                // ── BLOK IZLAZ ───────────────────────────────────
                col.Item().Element(c => BuildBlokNaslov(c, "IZLAZ", Colors.Red.Darken2));
                col.Item().Column(inner =>
                {
                    inner.Spacing(8);
                    inner.Item().Element(c => BuildIzlazTabela(c, "Gotova Roba i Sirovine",
                        data.IzlazGotovaRobaSirovine, Colors.Red.Lighten4));
                    inner.Item().Element(c => BuildIzlazTabela(c, "Ambalaza",
                        data.IzlazAmbalaza, Colors.Red.Lighten4));
                    inner.Item().Element(c => BuildIzlazTabela(c, "Repromaterijal",
                        data.IzlazRepromaterijal, Colors.Red.Lighten4));
                });

                var ukupnoIzlaz = data.IzlazGotovaRobaSirovine.Sum(x => x.Vrednost)
                    + data.IzlazAmbalaza.Sum(x => x.Vrednost)
                    + data.IzlazRepromaterijal.Sum(x => x.Vrednost);
                col.Item().Element(c => BuildUkupnoRed(c, "UKUPNO IZLAZ", ukupnoIzlaz, Colors.Red.Darken2));

                // ── BLOK FINANSIJE ────────────────────────────────
                col.Item().Element(c => BuildBlokNaslov(c, "FINANSIJE", Colors.Green.Darken2));
                col.Item().Element(c => BuildFinansijeTabela(c, data.Finansije));
            });
        }

        private void BuildBlokNaslov(IContainer container, string naziv, string boja)
        {
            container.Background(boja).Padding(6).Row(row =>
            {
                row.RelativeItem().Text($"▶  {naziv}")
                    .FontSize(11).Bold().FontColor(Colors.White);
            });
        }

        private void BuildUkupnoRed(IContainer container, string naziv, decimal vrednost, string boja)
        {
            container.Background(boja).Padding(5).Row(row =>
            {
                row.RelativeItem().Text(naziv).FontSize(10).Bold().FontColor(Colors.White);
                row.ConstantItem(120).AlignRight()
                    .Text(vrednost.ToString("N2", SrFormat) + " RSD")
                    .FontSize(10).Bold().FontColor(Colors.White);
            });
        }

        private void BuildUlazTabela(IContainer container, string podNaziv,
            List<PromenUlazModel> stavke, string headerBg)
        {
            container.Column(col =>
            {
                col.Item().Background(Colors.Blue.Darken1).Padding(4)
                    .Text(podNaziv).FontSize(9).Bold().FontColor(Colors.White);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(25);
                        cols.RelativeColumn(2);
                        cols.ConstantColumn(60);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(3);
                        cols.ConstantColumn(70);
                        cols.ConstantColumn(80);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Background(headerBg).Padding(3).Text("Rb.").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).Text("Dokument").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).Text("Datum").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).Text("Vrsta Proizvoda").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).Text("Dobavljač").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).AlignRight().Text("Količina").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).AlignRight().Text("Vrednost (RSD)").Bold().FontSize(7);
                    });

                    if (!stavke.Any())
                    {
                        table.Cell().ColumnSpan(7).Padding(4)
                            .Text("— nema podataka —").FontColor(Colors.Grey.Medium).Italic();
                    }
                    else
                    {
                        decimal suma = 0;
                        for (int i = 0; i < stavke.Count; i++)
                        {
                            var s = stavke[i];
                            var bg = i % 2 == 0 ? Colors.Blue.Lighten5 : Colors.White;

                            table.Cell().Background(bg).Padding(2).Text((i + 1).ToString()).FontSize(7);
                            table.Cell().Background(bg).Padding(2).Text(s.Dokument).FontSize(7);
                            table.Cell().Background(bg).Padding(2).Text(s.Datum.ToString("dd.MM.yyyy", SrFormat)).FontSize(7);
                            table.Cell().Background(bg).Padding(2).Text(s.VrstaProizvoda).FontSize(7);
                            table.Cell().Background(bg).Padding(2).Text(s.Dobavljac).FontSize(7);
                            table.Cell().Background(bg).Padding(2).AlignRight().Text(s.Kolicina.ToString("N2", SrFormat)).FontSize(7);
                            table.Cell().Background(bg).Padding(2).AlignRight().Text(s.Vrednost.ToString("N2", SrFormat)).FontSize(7);
                            suma += s.Vrednost;
                        }

                        // Suma red
                        table.Cell().ColumnSpan(6).Background(Colors.Blue.Lighten3).Padding(3)
                            .AlignRight().Text("Ukupno:").Bold().FontSize(7);
                        table.Cell().Background(Colors.Blue.Lighten3).Padding(3)
                            .AlignRight().Text(suma.ToString("N2", SrFormat)).Bold().FontSize(7);
                    }
                });
            });
        }

        private void BuildIzlazTabela(IContainer container, string podNaziv,
            List<PromenIzlazModel> stavke, string headerBg)
        {
            container.Column(col =>
            {
                col.Item().Background(Colors.Red.Darken1).Padding(4)
                    .Text(podNaziv).FontSize(9).Bold().FontColor(Colors.White);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(25);
                        cols.RelativeColumn(2);
                        cols.ConstantColumn(60);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(3);
                        cols.ConstantColumn(70);
                        cols.ConstantColumn(80);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Background(headerBg).Padding(3).Text("Rb.").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).Text("Dokument").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).Text("Datum").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).Text("Vrsta Proizvoda").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).Text("Kupac").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).AlignRight().Text("Količina").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).AlignRight().Text("Vrednost (RSD)").Bold().FontSize(7);
                    });

                    if (!stavke.Any())
                    {
                        table.Cell().ColumnSpan(7).Padding(4)
                            .Text("— nema podataka —").FontColor(Colors.Grey.Medium).Italic();
                    }
                    else
                    {
                        decimal suma = 0;
                        for (int i = 0; i < stavke.Count; i++)
                        {
                            var s = stavke[i];
                            var bg = i % 2 == 0 ? Colors.Red.Lighten5 : Colors.White;

                            table.Cell().Background(bg).Padding(2).Text((i + 1).ToString()).FontSize(7);
                            table.Cell().Background(bg).Padding(2).Text(s.Dokument).FontSize(7);
                            table.Cell().Background(bg).Padding(2).Text(s.Datum.ToString("dd.MM.yyyy", SrFormat)).FontSize(7);
                            table.Cell().Background(bg).Padding(2).Text(s.VrstaProizvoda).FontSize(7);
                            table.Cell().Background(bg).Padding(2).Text(s.Kupac).FontSize(7);
                            table.Cell().Background(bg).Padding(2).AlignRight().Text(s.Kolicina.ToString("N2", SrFormat)).FontSize(7);
                            table.Cell().Background(bg).Padding(2).AlignRight().Text(s.Vrednost.ToString("N2", SrFormat)).FontSize(7);
                            suma += s.Vrednost;
                        }

                        table.Cell().ColumnSpan(6).Background(Colors.Red.Lighten3).Padding(3)
                            .AlignRight().Text("Ukupno:").Bold().FontSize(7);
                        table.Cell().Background(Colors.Red.Lighten3).Padding(3)
                            .AlignRight().Text(suma.ToString("N2", SrFormat)).Bold().FontSize(7);
                    }
                });
            });
        }

        private void BuildFinansijeTabela(IContainer container, List<PromenFinansijeModel> stavke)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(25);
                    cols.RelativeColumn(2);
                    cols.ConstantColumn(60);
                    cols.ConstantColumn(70);
                    cols.RelativeColumn(4);
                    cols.ConstantColumn(90);
                    cols.ConstantColumn(90);
                });

                var hBg = Colors.Green.Lighten3;
                table.Header(h =>
                {
                    h.Cell().Background(hBg).Padding(3).Text("Rb.").Bold().FontSize(7);
                    h.Cell().Background(hBg).Padding(3).Text("Dokument").Bold().FontSize(7);
                    h.Cell().Background(hBg).Padding(3).Text("Datum").Bold().FontSize(7);
                    h.Cell().Background(hBg).Padding(3).Text("Tip Prometa").Bold().FontSize(7);
                    h.Cell().Background(hBg).Padding(3).Text("Komitent").Bold().FontSize(7);
                    h.Cell().Background(hBg).Padding(3).AlignRight().Text("Uplata (RSD)").Bold().FontSize(7);
                    h.Cell().Background(hBg).Padding(3).AlignRight().Text("Isplata (RSD)").Bold().FontSize(7);
                });

                if (!stavke.Any())
                {
                    table.Cell().ColumnSpan(7).Padding(4)
                        .Text("— nema podataka —").FontColor(Colors.Grey.Medium).Italic();
                }
                else
                {
                    decimal sumaUplata = 0, sumaIsplata = 0;
                    for (int i = 0; i < stavke.Count; i++)
                    {
                        var s = stavke[i];
                        var bg = i % 2 == 0 ? Colors.Green.Lighten5 : Colors.White;

                        table.Cell().Background(bg).Padding(2).Text((i + 1).ToString()).FontSize(7);
                        table.Cell().Background(bg).Padding(2).Text(s.Dokument).FontSize(7);
                        table.Cell().Background(bg).Padding(2).Text(s.Datum.ToString("dd.MM.yyyy", SrFormat)).FontSize(7);
                        table.Cell().Background(bg).Padding(2).Text(s.TipPrometa).FontSize(7);
                        table.Cell().Background(bg).Padding(2).Text(s.Komitent).FontSize(7);
                        table.Cell().Background(bg).Padding(2).AlignRight()
                            .Text(s.Uplata > 0 ? s.Uplata.ToString("N2", SrFormat) : "").FontSize(7).FontColor(Colors.Green.Darken2);
                        table.Cell().Background(bg).Padding(2).AlignRight()
                            .Text(s.Isplata > 0 ? s.Isplata.ToString("N2", SrFormat) : "").FontSize(7).FontColor(Colors.Red.Darken2);

                        sumaUplata += s.Uplata;
                        sumaIsplata += s.Isplata;
                    }

                    table.Cell().ColumnSpan(5).Background(Colors.Green.Lighten2).Padding(3)
                        .AlignRight().Text("Ukupno:").Bold().FontSize(7);
                    table.Cell().Background(Colors.Green.Lighten2).Padding(3)
                        .AlignRight().Text(sumaUplata.ToString("N2", SrFormat)).Bold().FontSize(7).FontColor(Colors.Green.Darken3);
                    table.Cell().Background(Colors.Green.Lighten2).Padding(3)
                        .AlignRight().Text(sumaIsplata.ToString("N2", SrFormat)).Bold().FontSize(7).FontColor(Colors.Red.Darken3);
                }
            });
        }
    }
}
