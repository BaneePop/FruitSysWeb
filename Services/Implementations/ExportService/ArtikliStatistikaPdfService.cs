using FruitSysWeb.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    public class ArtikliStatistikaPdfService
    {
        private static readonly CultureInfo SrFormat = new("sr-Latn-RS");

        public byte[] Generisi(List<ArtikliStatistikaGrupa> grupe, string naslovTaba, ArtikliStatistikaFilter filter)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                    page.Header().Element(c => BuildHeader(c, naslovTaba, filter));
                    page.Content().Element(c => BuildContent(c, grupe));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("ODETTA DOO • Artikli Statistika • ");
                        x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm", SrFormat)).FontColor(Colors.Grey.Medium);
                        x.Span(" • Strana ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        private void BuildHeader(IContainer container, string naslovTaba, ArtikliStatistikaFilter filter)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(inner =>
                    {
                        inner.Item().Text($"{naslovTaba.ToUpper()}")
                            .FontSize(15).Bold().FontColor(Colors.Blue.Darken3);
                        inner.Item().Text($"Period: {filter.OdDatum:dd.MM.yyyy} – {filter.DoDatum:dd.MM.yyyy}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                    row.ConstantItem(110).AlignRight().Column(inner =>
                    {
                        inner.Item().Text("ODETTA DOO").FontSize(10).Bold();
                        inner.Item().Text("PIB: 106784736").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
                col.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken3);
            });
        }

        private void BuildContent(IContainer container, List<ArtikliStatistikaGrupa> grupe)
        {
            container.Column(col =>
            {
                col.Spacing(10);
                foreach (var grupa in grupe)
                {
                    col.Item().Element(c => BuildGrupa(c, grupa));
                }
            });
        }

        private void BuildGrupa(IContainer container, ArtikliStatistikaGrupa grupa)
        {
            container.Column(col =>
            {
                // Naziv grupe
                col.Item().Background(Colors.Blue.Darken2).Padding(5).Row(row =>
                {
                    row.RelativeItem().Text(grupa.NazivGrupe.ToUpper())
                        .FontSize(10).Bold().FontColor(Colors.White);
                });

                // Tabela
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(4);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                    });

                    // Header
                    var headerBg = Colors.Blue.Lighten3;
                    table.Header(h =>
                    {
                        h.Cell().Background(headerBg).Padding(3).Text("Artikal").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).AlignRight().Text("Količina (kg)").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).AlignRight().Text("Prosečna cena").Bold().FontSize(7);
                        h.Cell().Background(headerBg).Padding(3).AlignRight().Text("Vrednost (RSD)").Bold().FontSize(7);
                    });

                    // Stavke
                    for (int i = 0; i < grupa.Stavke.Count; i++)
                    {
                        var s = grupa.Stavke[i];
                        var bg = i % 2 == 0 ? Colors.Blue.Lighten5 : Colors.White;

                        table.Cell().Background(bg).Padding(2).Text(s.Artikal).FontSize(7);
                        table.Cell().Background(bg).Padding(2).AlignRight()
                            .Text(s.Kolicina.ToString("N2", SrFormat)).FontSize(7);
                        table.Cell().Background(bg).Padding(2).AlignRight()
                            .Text(s.ProsecnaCena.ToString("N2", SrFormat)).FontSize(7);
                        table.Cell().Background(bg).Padding(2).AlignRight()
                            .Text(s.Vrednost.ToString("N2", SrFormat)).FontSize(7);
                    }

                    // Suma red
                    var sumBg = Colors.Blue.Darken1;
                    table.Cell().Background(sumBg).Padding(3)
                        .Text("UKUPNO").Bold().FontSize(7).FontColor(Colors.White);
                    table.Cell().Background(sumBg).Padding(3).AlignRight()
                        .Text(grupa.UkupnoKolicina.ToString("N2", SrFormat)).Bold().FontSize(7).FontColor(Colors.White);
                    table.Cell().Background(sumBg).Padding(3).AlignRight()
                        .Text(grupa.ProsecnaCenaUkupno.ToString("N2", SrFormat)).Bold().FontSize(7).FontColor(Colors.White);
                    table.Cell().Background(sumBg).Padding(3).AlignRight()
                        .Text(grupa.UkupnoVrednost.ToString("N2", SrFormat)).Bold().FontSize(7).FontColor(Colors.White);
                });
            });
        }
    }
}
