using FruitSysWeb.Models.IzvodDokumenata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    public class OtkupniListPdfService
    {
        private static readonly CultureInfo Sr = new("sr-Latn-RS");
        private static readonly string LogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo.png");

        private const string OdettaDrustvo = "Društvo za trgovinu i usluge";
        private const string OdettaNaziv = "ODETTA DOO, Kralja Dragutina 5, 7/31, 15000 Šabac, Srbija";
        private const string OdettaPib = "102679301";
        private const string OdettaMb = "17392344";
        private const string OdettaBanka = "AIK Banka:";
        private const string OdettaBankaVal = "105-14819-95";
        private const string OdettaRegBroj = "OB-2023/55291";
        private const string Napomena = "Napomena: Sva PVC Ambalaža mora biti vraćena najkasnije do kraja 9 meseca tekuće godine. Za ambalažu koja ne bude vraćena do tog datuma dobavljač ce biti zadužen 150 din/ kom za PVC 4/1 i 6/1 i 300 din/kom za PVC 12/1.";

        public byte[] Generisi(IzvodOtkupniListDetalji model)
        {
            return Document.Create(container =>
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
            }).GeneratePdf();
        }

        private void BuildHeader(IContainer container, IzvodOtkupniListDetalji model)
        {
            container.Column(col =>
            {
                // Logo + kompanija
                col.Item().PaddingBottom(6).Row(row =>
                {
                    if (File.Exists(LogoPath))
                        row.ConstantItem(90).PaddingRight(10).AlignMiddle().Image(LogoPath).FitArea();

                    row.RelativeItem().AlignMiddle().Column(c =>
                    {
                        c.Item().Text(OdettaDrustvo).FontSize(8).FontColor(Colors.Grey.Darken2);
                        c.Item().Text(OdettaNaziv).FontSize(9).Bold();
                        c.Item().PaddingTop(2).Row(r =>
                        {
                            r.RelativeItem().Text(t =>
                            {
                                t.Span("PIB:").FontSize(8).FontColor(Colors.Grey.Darken2);
                                t.Span($"          {OdettaPib}").FontSize(8);
                            });
                            r.RelativeItem().AlignRight().Text(t =>
                            {
                                t.Span($"{OdettaBanka} ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                t.Span(OdettaBankaVal).FontSize(8);
                            });
                        });
                        c.Item().Text(t =>
                        {
                            t.Span("Matični broj:").FontSize(8).FontColor(Colors.Grey.Darken2);
                            t.Span($"  {OdettaMb}").FontSize(8);
                        });
                        c.Item().Text(t =>
                        {
                            t.Span("Registarski broj:").FontSize(8).FontColor(Colors.Grey.Darken2);
                            t.Span($"  {OdettaRegBroj}").FontSize(8);
                        });
                    });
                });

                // Naslov + Otkupno mesto
                col.Item().PaddingBottom(4).Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("PRIZNANICA - OTKUPNI LIST BR.  ").FontSize(11).Bold();
                        t.Span($"{model.Sifra}").FontSize(11).Bold();
                        if (!string.IsNullOrEmpty(model.OtkupnoMesto))
                            t.Span($"  {model.OtkupnoMesto}").FontSize(11).Bold();
                    });
                    row.ConstantItem(150).AlignRight().Text(t =>
                    {
                        t.Span("Otkupno mesto:  ").FontSize(8).FontColor(Colors.Grey.Darken2);
                        t.Span(model.OtkupnoMesto).FontSize(8);
                    });
                });

                // Datum prometa + Datum izdavanja
                col.Item().PaddingBottom(4).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Row(row =>
                {
                    row.RelativeItem().Row(r =>
                    {
                        r.ConstantItem(120).Text("Datum prometa dobara:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        r.ConstantItem(90).BorderBottom(0.5f).BorderColor(Colors.Grey.Darken2)
                            .Text(model.DatumPrometaFormatted).FontSize(8);
                    });
                    row.RelativeItem().AlignRight().Row(r =>
                    {
                        r.ConstantItem(130).AlignRight().Text("Datum i mesto izdavanja:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        r.ConstantItem(70).AlignRight().BorderBottom(0.5f).BorderColor(Colors.Grey.Darken2)
                            .Text(model.DatumFormatted).FontSize(8);
                    });
                });

                // Primalac - naslov
                col.Item().PaddingTop(4).Text("Primalac priznanice-poljoprivredni proizvođač").FontSize(9);

                // Ime + JMBG
                col.Item().PaddingTop(3).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Row(row =>
                {
                    row.ConstantItem(80).Text("Ime i prezime:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.RelativeItem().Text(model.Komitent).FontSize(9).Bold();
                    row.ConstantItem(35).Text("JMBG:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.ConstantItem(110).Text(model.JMBG).FontSize(9).Bold();
                });

                // Adresa
                col.Item().PaddingTop(3).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Row(row =>
                {
                    row.ConstantItem(40).Text("Adresa:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.RelativeItem().Text(model.KomitentAdresa).FontSize(8);
                });

                // Tekući račun
                if (!string.IsNullOrEmpty(model.TekuciRacun))
                {
                    col.Item().PaddingTop(3).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Row(row =>
                    {
                        row.ConstantItem(80).Text("Tekući račun:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.RelativeItem().Text(model.TekuciRacun).FontSize(8);
                    });
                }
            });
        }

        private void BuildContent(IContainer container, IzvodOtkupniListDetalji model)
        {
            container.Column(col =>
            {
                col.Spacing(6);

                // Tabela robe (vrsta dobra)
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(22);    // RB
                        cols.RelativeColumn(5);     // VRSTA DOBRA
                        cols.RelativeColumn(1);     // JM
                        cols.RelativeColumn(1.8f);  // KOLIČINA
                        cols.RelativeColumn(1.8f);  // CENA
                        cols.RelativeColumn(2.2f);  // IZNOS NAKNADE
                    });

                    table.Header(h =>
                    {
                        HeaderCell(h, "RB");
                        HeaderCell(h, "VRSTA DOBRA");
                        HeaderCell(h, "JM");
                        HeaderCell(h, "KOLIČINA");
                        HeaderCell(h, "CENA");
                        HeaderCell(h, "IZNOS NAKNADE");
                    });

                    int rb = 1;
                    foreach (var s in model.Stavke)
                    {
                        DataCellCenter(table, rb.ToString());
                        DataCellLeft(table, s.Artikal);
                        DataCellCenter(table, s.JM);
                        DataCellRight(table, s.KolicinaFormatted);
                        DataCellRight(table, s.CenaFormatted);
                        DataCellRight(table, s.IznosFormatted);
                        rb++;
                    }
                    // Prazni redovi do minimalno 3
                    for (int i = model.Stavke.Count; i < 3; i++)
                    {
                        DataCellCenter(table, string.Empty);
                        DataCellLeft(table, string.Empty);
                        DataCellCenter(table, string.Empty);
                        DataCellRight(table, string.Empty);
                        DataCellRight(table, string.Empty);
                        DataCellRight(table, string.Empty);
                    }
                });

                // Finansijski rezime
                col.Item().Element(c => BuildFinansijskiRezime(c, model));

                // Specifikacija
                col.Item().Element(c => BuildSpecifikacija(c, model));

                // Ambalaža
                if (model.Ambalaza.Count > 0)
                    col.Item().Element(c => BuildAmbalaza(c, model.Ambalaza));

                // Potpisi
                col.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Darken2).PaddingBottom(20).Text(string.Empty);
                        c.Item().PaddingTop(2).Text("Za otkupljivača").FontSize(8).FontColor(Colors.Grey.Darken2);
                    });
                    row.ConstantItem(30);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Darken2).PaddingBottom(20).Text(string.Empty);
                        c.Item().PaddingTop(2).Text("Potpis dobavljača i primaoca isplate").FontSize(8).FontColor(Colors.Grey.Darken2);
                    });
                });
            });
        }

        private void BuildFinansijskiRezime(IContainer container, IzvodOtkupniListDetalji model)
        {
            container.Column(col =>
            {
                void Red(string naziv, string vrednost, bool bold = false)
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(200).Text(naziv).FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.ConstantItem(100).Text(vrednost).FontSize(bold ? 9 : 8).Bold();
                        if (bold)
                            row.RelativeItem();
                    });
                }

                void RedDva(string naziv1, string vrednost1, string naziv2, string vrednost2)
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(200).Text(naziv1).FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.ConstantItem(100).Text(vrednost1).FontSize(8);
                        row.ConstantItem(130).AlignRight().Text(naziv2).FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.RelativeItem().AlignRight().Text(vrednost2).FontSize(8);
                    });
                }

                Red("Svega vrednost primljenih dobara i usluga:", model.IznosOsnovice.ToString("N2", Sr));

                RedDva(
                    $"Stopa PDV nadoknade:",
                    $"{model.StopaPDV:N2} %",
                    "Datum isplate PDV nadoknade:",
                    string.Empty
                );

                RedDva(
                    "Iznos PDV nadoknade:",
                    model.IznosPDV.ToString("N2", Sr),
                    "Datum isplate osnovice:",
                    string.Empty
                );

                // Ukupan iznos + Rok isplate
                col.Item().Row(row =>
                {
                    row.ConstantItem(200).Text("Ukupan iznos za isplatu:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.ConstantItem(100).Text(model.IznosUkupno.ToString("N2", Sr)).FontSize(9).Bold();
                    row.ConstantItem(60).AlignRight().Text("Rok isplate:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    row.RelativeItem().AlignRight().Text(model.RokIsplate).FontSize(8);
                });

                // Način isplate
                col.Item().PaddingTop(2).Row(row =>
                {
                    row.ConstantItem(120).Text("Način isplate osnovice:").FontSize(8).FontColor(Colors.Grey.Darken2);
                    // Checkbox stil
                    row.ConstantItem(60).Border(0.5f).BorderColor(Colors.Grey.Darken2).Padding(2)
                        .AlignCenter().Text("1. Gotovina").FontSize(8);
                    row.ConstantItem(8);
                    row.ConstantItem(70).Text("2. Virmanski").FontSize(8);
                    row.RelativeItem();
                });

                // Poreski broj + Broj polj. gazdinstva
                if (!string.IsNullOrEmpty(model.PoreskiBroj))
                {
                    col.Item().PaddingTop(3).Row(row =>
                    {
                        row.ConstantItem(80).Text("Poreski broj:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.RelativeItem().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Text(model.PoreskiBroj).FontSize(8);
                    });
                    col.Item().PaddingTop(2).Row(row =>
                    {
                        row.ConstantItem(170).Text("Broj registrovanog polj. gazdinstva:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        row.RelativeItem().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Text(model.PoreskiBroj).FontSize(8);
                    });
                }
            });
        }

        private void BuildSpecifikacija(IContainer container, IzvodOtkupniListDetalji model)
        {
            container.Column(col =>
            {
                col.Item().Text("Specifikacija:").FontSize(9);
                col.Item().PaddingTop(2).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(22);
                        cols.RelativeColumn(5);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1.2f);
                    });

                    table.Header(h =>
                    {
                        HeaderCell(h, "RB");
                        HeaderCell(h, "NAZIV ARTIKLA");
                        HeaderCell(h, "JM");
                        HeaderCell(h, "KOLICINA");
                        HeaderCell(h, "%");
                    });

                    var ukupnoSpec = model.Stavke.Sum(x => x.Kolicina);
                    int rb = 1;
                    foreach (var s in model.Stavke)
                    {
                        DataCellCenter(table, rb.ToString());
                        DataCellLeft(table, s.Artikal);
                        DataCellCenter(table, s.JM);
                        DataCellRight(table, s.KolicinaFormatted);
                        var proc = ukupnoSpec > 0 ? s.Kolicina / ukupnoSpec * 100 : 0;
                        DataCellRight(table, proc.ToString("N1", Sr));
                        rb++;
                    }
                });
            });
        }

        private void BuildAmbalaza(IContainer container, List<IzvodOtkupniListAmbalaza> ambalaza)
        {
            container.Column(col =>
            {
                col.Item().Text("Ambalaza:").FontSize(9);
                col.Item().PaddingTop(2).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(22);
                        cols.RelativeColumn(5);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                    });

                    table.Header(h =>
                    {
                        HeaderCell(h, "RB");
                        HeaderCell(h, "NAZIV ARTIKLA");
                        HeaderCell(h, "JM");
                        HeaderCell(h, "PRIMLJENO");
                        HeaderCell(h, "IZDATO");
                    });

                    int rb = 1;
                    foreach (var a in ambalaza)
                    {
                        DataCellCenter(table, rb.ToString());
                        DataCellLeft(table, a.Naziv);
                        DataCellCenter(table, "kom");
                        DataCellRight(table, a.KolicinaFormatted);
                        DataCellRight(table, a.IznosFormatted);
                        rb++;
                    }
                });
            });
        }

        private void BuildFooter(IContainer container)
        {
            container.BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(4)
                .Text(Napomena).FontSize(7).FontColor(Colors.Grey.Darken1);
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
    }
}
