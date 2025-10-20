using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using FruitSysWeb.Models.Sledljivost;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class SledljivostPdfService
    {
        public SledljivostPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerisiUpstreamPdf(SledljivostModel sledljivost)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    // Header
                    page.Header().Column(column =>
                    {
                        column.Item().BorderBottom(2).BorderColor(Colors.Blue.Darken2).PaddingBottom(10).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("SLEDLJIVOST - UPSTREAM")
                                    .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                                col.Item().Text($"Radni Nalog: {sledljivost.RadniNalog?.Sifra ?? "N/A"}")
                                    .FontSize(12).SemiBold();
                            });

                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text($"Datum: {DateTime.Now:dd.MM.yyyy}")
                                    .FontSize(10);
                                col.Item().Text($"Kupac: {sledljivost.RadniNalog?.KomitentNaziv ?? "N/A"}")
                                    .FontSize(10);
                            });
                        });

                        column.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Text($"Količina: {sledljivost.RadniNalog?.Kolicina?.ToString("F2") ?? "0"} kg").FontSize(11);
                            row.RelativeItem().Text($"Broj pakovanja: {sledljivost.RadniNalog?.BrojPakovanja?.ToString() ?? "0"}").FontSize(11);
                        });
                    });

                    // Content - Flowchart
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        // FAZA 1: NABAVKA
                        column.Item().PaddingBottom(15).Column(col =>
                        {
                            // Naslov faze
                            col.Item().Background(Colors.Red.Lighten3)
                                .Padding(8)
                                .Text("FAZA 1: NABAVKA SIROVINE")
                                .FontSize(13).Bold().FontColor(Colors.White);

                            col.Item().PaddingTop(10);

                            // Prijemnice
                            if (sledljivost.Prijemnice?.Any() == true)
                            {
                                col.Item().Row(row =>
                                {
                                    foreach (var prijemnica in sledljivost.Prijemnice.Take(3)) // Max 3 na red
                                    {
                                        row.RelativeItem().PaddingRight(10).Column(innerCol =>
                                        {
                                            // Prijemnica box
                                            innerCol.Item().Border(2).BorderColor(Colors.Red.Medium)
                                                .Background(Colors.Red.Lighten5)
                                                .Padding(10).Column(boxCol =>
                                                {
                                                    boxCol.Item().Text(prijemnica.Sifra ?? "N/A").Bold().FontSize(11);
                                                    boxCol.Item().Text($"{prijemnica.Datum?.ToString("dd.MM.yyyy") ?? "N/A"}").FontSize(9);
                                                    boxCol.Item().Text($"Dobavljač: {prijemnica.KomitentNaziv ?? "N/A"}").FontSize(8);

                                                    // Paletni listovi
                                                    var palete = sledljivost.PaletniListoviUlaz?
                                                        .Where(pl => pl.PrijemnicaSifra == prijemnica.Sifra)
                                                        .Take(3)
                                                        .ToList();

                                                    if (palete?.Any() == true)
                                                    {
                                                        boxCol.Item().PaddingTop(5).PaddingLeft(10).Column(plCol =>
                                                        {
                                                            foreach (var pl in palete)
                                                            {
                                                                plCol.Item().Text($"▸ {pl.Sifra} ({pl.Tezina?.ToString("F0") ?? "0"} kg)")
                                                                    .FontSize(8).FontColor(Colors.Grey.Darken2);
                                                            }
                                                        });
                                                    }
                                                });

                                            // Strelica dole
                                            innerCol.Item().PaddingTop(5).AlignCenter().Text("▼").FontSize(16).FontColor(Colors.Blue.Medium);
                                        });
                                    }
                                });
                            }
                        });

                        // FAZA 2: PROIZVODNJA
                        column.Item().PaddingBottom(15).Column(col =>
                        {
                            // Naslov faze
                            col.Item().Background(Colors.Blue.Medium)
                                .Padding(8)
                                .Text("FAZA 2: PROIZVODNJA")
                                .FontSize(13).Bold().FontColor(Colors.White);

                            col.Item().PaddingTop(10);

                            // Evidencije Rada
                            if (sledljivost.EvidencijeRada?.Any() == true)
                            {
                                foreach (var evidencija in sledljivost.EvidencijeRada)
                                {
                                    col.Item().PaddingBottom(10).AlignCenter().Column(evCol =>
                                    {
                                        // Evidencija box
                                        evCol.Item().MaxWidth(400).Border(2).BorderColor(Colors.Blue.Medium)
                                            .Background(Colors.Blue.Lighten5)
                                            .Padding(10).Column(boxCol =>
                                            {
                                                boxCol.Item().AlignCenter().Text(evidencija.Sifra ?? "N/A").Bold().FontSize(11);
                                                boxCol.Item().AlignCenter().Text($"{evidencija.Datum?.ToString("dd.MM.yyyy") ?? "N/A"} | Smena {evidencija.Smena ?? 0}")
                                                    .FontSize(9);

                                                // Utrošeni paletni listovi
                                                var utroseni = sledljivost.PaletniListoviUlaz?
                                                    .Where(pl => evidencija.UtroseniPaletniListoviIDs?.Contains(pl.ID) == true)
                                                    .Take(5)
                                                    .ToList();

                                                if (utroseni?.Any() == true)
                                                {
                                                    boxCol.Item().PaddingTop(5).Column(utrCol =>
                                                    {
                                                        utrCol.Item().Text("Utrošeno:").FontSize(9).Italic();
                                                        foreach (var pl in utroseni)
                                                        {
                                                            utrCol.Item().PaddingLeft(10).Text($"• {pl.Sifra} ({pl.Tezina?.ToString("F0") ?? "0"} kg)")
                                                                .FontSize(8);
                                                        }
                                                    });
                                                }

                                                // Proizvedeni paletni listovi
                                                var proizvedeni = sledljivost.PaletniListoviIzlaz?
                                                    .Where(pl => pl.EvidencijaRadaID == evidencija.ID)
                                                    .Take(5)
                                                    .ToList();

                                                if (proizvedeni?.Any() == true)
                                                {
                                                    boxCol.Item().PaddingTop(5).Column(prodCol =>
                                                    {
                                                        prodCol.Item().Text("Proizvedeno:").FontSize(9).Italic().FontColor(Colors.Green.Darken1);
                                                        foreach (var pl in proizvedeni)
                                                        {
                                                            prodCol.Item().PaddingLeft(10).Text($"★ {pl.Sifra} ({pl.Tezina?.ToString("F0") ?? "0"} kg)")
                                                                .FontSize(8).FontColor(Colors.Green.Darken2).Bold();
                                                        }
                                                    });
                                                }
                                            });

                                        // Strelica dole
                                        evCol.Item().PaddingTop(5).AlignCenter().Text("▼").FontSize(16).FontColor(Colors.Green.Medium);
                                    });
                                }
                            }
                        });

                        // FAZA 3: PRODAJA
                        column.Item().Column(col =>
                        {
                            // Naslov faze
                            col.Item().Background(Colors.Green.Medium)
                                .Padding(8)
                                .Text("FAZA 3: PRODAJA")
                                .FontSize(13).Bold().FontColor(Colors.White);

                            col.Item().PaddingTop(10);

                            // Otpremnice
                            if (sledljivost.Otpremnice?.Any() == true)
                            {
                                col.Item().Row(row =>
                                {
                                    foreach (var otpremnica in sledljivost.Otpremnice.Take(3))
                                    {
                                        row.RelativeItem().PaddingRight(10).Column(otpCol =>
                                        {
                                            // Otpremnica box
                                            otpCol.Item().Border(2).BorderColor(Colors.Green.Medium)
                                                .Background(Colors.Green.Lighten5)
                                                .Padding(10).Column(boxCol =>
                                                {
                                                    boxCol.Item().Text(otpremnica.Sifra ?? "N/A").Bold().FontSize(11);
                                                    boxCol.Item().Text($"{otpremnica.Datum?.ToString("dd.MM.yyyy") ?? "N/A"}").FontSize(9);
                                                    boxCol.Item().Text($"Kupac: {otpremnica.KomitentNaziv ?? "N/A"}").FontSize(8);

                                                    // Paletni listovi
                                                    var palete = sledljivost.PaletniListoviIzlaz?
                                                        .Where(pl => pl.OtpremnicaSifra == otpremnica.Sifra)
                                                        .Take(3)
                                                        .ToList();

                                                    if (palete?.Any() == true)
                                                    {
                                                        boxCol.Item().PaddingTop(5).PaddingLeft(10).Column(plCol =>
                                                        {
                                                            foreach (var pl in palete)
                                                            {
                                                                plCol.Item().Text($"▸ {pl.Sifra} ({pl.Tezina?.ToString("F0") ?? "0"} kg)")
                                                                    .FontSize(8).FontColor(Colors.Grey.Darken2);
                                                            }
                                                        });
                                                    }
                                                });
                                        });
                                    }
                                });
                            }
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Generisano: ").FontSize(8);
                        text.Span($"{DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).SemiBold();
                        text.Span(" | Stranica ");
                        text.CurrentPageNumber().FontSize(8);
                        text.Span(" od ");
                        text.TotalPages().FontSize(8);
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerisiDownstreamPdf(SledljivostModel sledljivost)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    // Header
                    page.Header().Column(column =>
                    {
                        column.Item().BorderBottom(2).BorderColor(Colors.Green.Darken2).PaddingBottom(10).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("SLEDLJIVOST - DOWNSTREAM")
                                    .FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                                col.Item().Text($"Paletni List: {sledljivost.Sifra ?? "N/A"}")
                                    .FontSize(12).SemiBold();
                            });

                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text($"Datum: {DateTime.Now:dd.MM.yyyy}")
                                    .FontSize(10);
                            });
                        });
                    });

                    // Content - Reverse Flowchart
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        // FAZA 1: PRODAJA (Gore)
                        column.Item().PaddingBottom(15).Column(col =>
                        {
                            col.Item().Background(Colors.Green.Medium)
                                .Padding(8)
                                .Text("GDE JE PRODAT")
                                .FontSize(13).Bold().FontColor(Colors.White);

                            col.Item().PaddingTop(10);

                            // Radni Nalog i Otpremnica
                            col.Item().AlignCenter().Column(prodCol =>
                            {
                                if (sledljivost.RadniNalog != null)
                                {
                                    prodCol.Item().MaxWidth(400).Border(2).BorderColor(Colors.Blue.Medium)
                                        .Background(Colors.Blue.Lighten5)
                                        .Padding(10).Column(rnCol =>
                                        {
                                            rnCol.Item().AlignCenter().Text($"Radni Nalog: {sledljivost.RadniNalog.Sifra}").Bold().FontSize(11);
                                            rnCol.Item().AlignCenter().Text($"Kupac: {sledljivost.RadniNalog.KomitentNaziv}").FontSize(9);
                                        });
                                    prodCol.Item().PaddingTop(5).AlignCenter().Text("▼").FontSize(16).FontColor(Colors.Green.Medium);
                                }

                                if (sledljivost.Otpremnice?.Any() == true)
                                {
                                    var otpremnica = sledljivost.Otpremnice.First();
                                    prodCol.Item().MaxWidth(400).Border(2).BorderColor(Colors.Green.Medium)
                                        .Background(Colors.Green.Lighten5)
                                        .Padding(10).Column(otpCol =>
                                        {
                                            otpCol.Item().AlignCenter().Text(otpremnica.Sifra ?? "N/A").Bold().FontSize(11);
                                            otpCol.Item().AlignCenter().Text($"Kupac: {otpremnica.KomitentNaziv ?? "N/A"}").FontSize(9);

                                            // Glavni paletni list
                                            var palete = sledljivost.PaletniListoviIzlaz?.Take(3).ToList();
                                            if (palete?.Any() == true)
                                            {
                                                otpCol.Item().PaddingTop(5).Column(plCol =>
                                                {
                                                    foreach (var pl in palete)
                                                    {
                                                        plCol.Item().AlignCenter().Text($"★ {pl.Sifra} ({pl.Tezina?.ToString("F0") ?? "0"} kg)")
                                                            .FontSize(9).FontColor(Colors.Green.Darken2).Bold();
                                                    }
                                                });
                                            }
                                        });
                                }

                                prodCol.Item().PaddingTop(10).AlignCenter().Text("▼").FontSize(16).FontColor(Colors.Blue.Medium);
                            });
                        });

                        // FAZA 2: PROIZVODNJA
                        column.Item().PaddingBottom(15).Column(col =>
                        {
                            col.Item().Background(Colors.Blue.Medium)
                                .Padding(8)
                                .Text("KAKO JE PROIZVEDEN")
                                .FontSize(13).Bold().FontColor(Colors.White);

                            col.Item().PaddingTop(10);

                            if (sledljivost.EvidencijeRada?.Any() == true)
                            {
                                var evidencija = sledljivost.EvidencijeRada.First();
                                col.Item().AlignCenter().MaxWidth(400).Border(2).BorderColor(Colors.Blue.Medium)
                                    .Background(Colors.Blue.Lighten5)
                                    .Padding(10).Column(evCol =>
                                    {
                                        evCol.Item().AlignCenter().Text(evidencija.Sifra ?? "N/A").Bold().FontSize(11);
                                        evCol.Item().AlignCenter().Text($"{evidencija.Datum?.ToString("dd.MM.yyyy") ?? "N/A"} | Smena {evidencija.Smena ?? 0}")
                                            .FontSize(9);

                                        // Utrošeni paletni listovi (sirovine)
                                        var utroseni = sledljivost.PaletniListoviUlaz?.Take(5).ToList();
                                        if (utroseni?.Any() == true)
                                        {
                                            evCol.Item().PaddingTop(5).Column(utrCol =>
                                            {
                                                utrCol.Item().AlignCenter().Text("Utrošeno:").FontSize(9).Italic();
                                                foreach (var pl in utroseni)
                                                {
                                                    utrCol.Item().AlignCenter().Text($"• {pl.Sifra} ({pl.Tezina?.ToString("F0") ?? "0"} kg)")
                                                        .FontSize(8);
                                                }
                                            });
                                        }
                                    });

                                col.Item().PaddingTop(10).AlignCenter().Text("▼").FontSize(16).FontColor(Colors.Red.Medium);
                            }
                        });

                        // FAZA 3: NABAVKA
                        column.Item().Column(col =>
                        {
                            col.Item().Background(Colors.Red.Lighten3)
                                .Padding(8)
                                .Text("ODAKLE DOLAZI (Nabavka sirovine)")
                                .FontSize(13).Bold().FontColor(Colors.White);

                            col.Item().PaddingTop(10);

                            if (sledljivost.Prijemnice?.Any() == true)
                            {
                                col.Item().Row(row =>
                                {
                                    foreach (var prijemnica in sledljivost.Prijemnice.Take(3))
                                    {
                                        row.RelativeItem().PaddingRight(10).Column(prCol =>
                                        {
                                            prCol.Item().Border(2).BorderColor(Colors.Red.Medium)
                                                .Background(Colors.Red.Lighten5)
                                                .Padding(10).Column(boxCol =>
                                                {
                                                    boxCol.Item().Text(prijemnica.Sifra ?? "N/A").Bold().FontSize(11);
                                                    boxCol.Item().Text($"{prijemnica.Datum?.ToString("dd.MM.yyyy") ?? "N/A"}").FontSize(9);
                                                    boxCol.Item().Text($"Dobavljač: {prijemnica.KomitentNaziv ?? "N/A"}").FontSize(8);

                                                    // Paletni listovi sirovine
                                                    var palete = sledljivost.PaletniListoviUlaz?
                                                        .Where(pl => pl.PrijemnicaSifra == prijemnica.Sifra)
                                                        .Take(3)
                                                        .ToList();

                                                    if (palete?.Any() == true)
                                                    {
                                                        boxCol.Item().PaddingTop(5).PaddingLeft(10).Column(plCol =>
                                                        {
                                                            foreach (var pl in palete)
                                                            {
                                                                plCol.Item().Text($"▸ {pl.Sifra} ({pl.Tezina?.ToString("F0") ?? "0"} kg)")
                                                                    .FontSize(8).FontColor(Colors.Grey.Darken2);
                                                            }
                                                        });
                                                    }
                                                });
                                        });
                                    }
                                });
                            }
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Generisano: ").FontSize(8);
                        text.Span($"{DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).SemiBold();
                        text.Span(" | Stranica ");
                        text.CurrentPageNumber().FontSize(8);
                        text.Span(" od ");
                        text.TotalPages().FontSize(8);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
