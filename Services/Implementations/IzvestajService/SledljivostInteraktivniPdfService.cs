using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using FruitSysWeb.Models.Sledljivost;
using System.Globalization;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    /// <summary>
    /// Interaktivni PDF servis za sledljivost sa bookmarks, hyperlinkovima i strukturom na više strana
    /// </summary>
    public class SledljivostInteraktivniPdfService
    {
        private static readonly CultureInfo SrpskiFormat = new CultureInfo("sr-Latn-RS");

        public SledljivostInteraktivniPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerisiInteraktivniUpstreamPdf(SledljivostModel sledljivost)
        {
            var document = Document.Create(container =>
            {
                // STRANA 1: PREGLED SVIH PODATAKA
                container.Page(page =>
                {
                    ConfigurePage(page);

                    page.Header().Element(c => Header(c, "SLEDLJIVOST - UPSTREAM (Pregled)", sledljivost.RadniNalog?.Sifra));

                    page.Content().Column(column =>
                    {
                        // BOOKMARK - označi destinaciju
                        column.Item().Section("strana1");

                        column.Item().Text("PREGLED SVIH PODATAKA").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        column.Item().PaddingBottom(10);

                        // 1. Radni Nalog Podaci
                        if (sledljivost.RadniNalog != null)
                        {
                            column.Item().Element(c => PodaciRadnogNaloga(c, sledljivost.RadniNalog));
                        }

                        // 2. Otpremnice Podaci
                        if (sledljivost.Otpremnice?.Any() == true)
                        {
                            column.Item().PaddingTop(15).Element(c => PodaciOtpremnica(c, sledljivost.Otpremnice));
                        }

                        // 3. Evidencije Rada Podaci
                        if (sledljivost.EvidencijeRada?.Any() == true)
                        {
                            column.Item().PaddingTop(15).Element(c => PodaciEvidencijaRada(c, sledljivost.EvidencijeRada));
                        }

                        // 4. Prijemnice Podaci
                        if (sledljivost.Prijemnice?.Any() == true)
                        {
                            column.Item().PaddingTop(15).Element(c => PodaciPrijemnica(c, sledljivost.Prijemnice, sledljivost.PaletniListoviUlaz));
                        }
                    });

                    page.Footer().Element(Footer);
                });

                // STRANA 2: DIJAGRAM - RN, EVIDENCIJE, PALETNI LISTOVI GOTOVIH PROIZVODA
                container.Page(page =>
                {
                    ConfigurePage(page);

                    page.Header().Element(c => Header(c, "DIJAGRAM - PROIZVODNJA", sledljivost.RadniNalog?.Sifra));

                    page.Content().Column(column =>
                    {
                        column.Item().Section("strana2");

                        column.Item().Text("STRANA 2: DIJAGRAM PROIZVODNJE").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                        column.Item().PaddingBottom(10);

                        // Radni Nalog (vrh)
                        if (sledljivost.RadniNalog != null)
                        {
                            column.Item().Element(c => RadniNalogBoks(c, sledljivost.RadniNalog));
                        }

                        column.Item().PaddingVertical(10).AlignCenter().Text("▼").FontSize(20).FontColor(Colors.Blue.Medium);

                        // Evidencije Rada sa povezanim paletnim listovima
                        if (sledljivost.EvidencijeRada?.Any() == true)
                        {
                            column.Item().Element(c => EvidencijeRadaDijagram(c, sledljivost.EvidencijeRada, sledljivost.PaletniListoviIzlaz));
                        }
                    });

                    page.Footer().Element(Footer);
                });

                // STRANA 3: DIJAGRAM - PRIJEMNICE SA PALETNIM LISTOVIMA SIROVINE
                container.Page(page =>
                {
                    ConfigurePage(page);

                    page.Header().Element(c => Header(c, "DIJAGRAM - NABAVKA SIROVINE", sledljivost.RadniNalog?.Sifra));

                    page.Content().Column(column =>
                    {
                        column.Item().Section("strana3");

                        column.Item().Text("STRANA 3: NABAVKA SIROVINE").FontSize(14).Bold().FontColor(Colors.Red.Darken2);
                        column.Item().PaddingBottom(15);

                        // Prijemnice sa paletnim listovima
                        if (sledljivost.Prijemnice?.Any() == true)
                        {
                            column.Item().Element(c => PrijemniceDijagram(c, sledljivost.Prijemnice, sledljivost.PaletniListoviUlaz));
                        }
                    });

                    page.Footer().Element(Footer);
                });

                // STRANA 4: OTPREMNICA SA PALETNIM LISTOVIMA GOTOVE ROBE
                container.Page(page =>
                {
                    ConfigurePage(page);

                    page.Header().Element(c => Header(c, "DIJAGRAM - PRODAJA", sledljivost.RadniNalog?.Sifra));

                    page.Content().Column(column =>
                    {
                        column.Item().Section("strana4");

                        column.Item().Text("STRANA 4: PRODAJA - OTPREMNICE").FontSize(14).Bold().FontColor(Colors.Green.Darken2);
                        column.Item().PaddingBottom(15);

                        // Otpremnice sa paletnim listovima
                        if (sledljivost.Otpremnice?.Any() == true)
                        {
                            column.Item().Element(c => OtpremniceDijagram(c, sledljivost.Otpremnice, sledljivost.PaletniListoviIzlaz));
                        }
                    });

                    page.Footer().Element(Footer);
                });
            });

            return document.GeneratePdf();
        }

        #region Helper Methods - Page Configuration

        private void ConfigurePage(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));
        }

        private void Header(IContainer container, string naslov, string? radniNalogSifra)
        {
            container.BorderBottom(2).BorderColor(Colors.Blue.Darken2).PaddingBottom(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(naslov)
                        .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                    if (!string.IsNullOrEmpty(radniNalogSifra))
                    {
                        col.Item().Text($"Radni Nalog: {radniNalogSifra}")
                            .FontSize(11).SemiBold();
                    }
                });

                row.ConstantItem(120).AlignRight().Column(col =>
                {
                    col.Item().Text($"Datum: {DateTime.Now:dd.MM.yyyy}").FontSize(9);
                    col.Item().Text($"Vreme: {DateTime.Now:HH:mm}").FontSize(9);
                });
            });
        }

        private void Footer(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("ODETTA DOO | ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span("Generisano: ").FontSize(8);
                text.Span($"{DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).SemiBold();
                text.Span(" | Stranica ");
                text.CurrentPageNumber().FontSize(8);
                text.Span(" / ");
                text.TotalPages().FontSize(8);
            });
        }

        #endregion

        #region STRANA 1 - Pregled Podataka

        private void PodaciRadnogNaloga(IContainer container, RadniNalogDetalji rn)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Blue.Lighten4).Padding(8)
                    .Text("RADNI NALOG - OSNOVNI PODACI").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);

                column.Item().Border(1).BorderColor(Colors.Blue.Medium).Padding(10).Column(col =>
                {
                    // Reference na stranu 2
                    col.Item().Text(t =>
                    {
                        t.Span($"Šifra: {rn.Sifra}").FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                        t.Span(" → vidi Stranu 2").FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                    });

                    col.Item().Text($"Kupac: {rn.KomitentNaziv ?? "N/A"}").FontSize(10);
                    col.Item().Text($"Lot Naloga: {rn.LotNaloga ?? "N/A"}").FontSize(10);
                    col.Item().Text($"Količina: {FormatDecimal(rn.Kolicina)} kg").FontSize(10);
                    col.Item().Text($"Broj Pakovanja: {rn.BrojPakovanja ?? 0}").FontSize(10);
                    col.Item().Text($"Datum: {FormatDate(rn.Datum)}").FontSize(10);
                    col.Item().Text($"Status: {rn.StatusNaziv ?? "N/A"}").FontSize(10);
                });
            });
        }

        private void PodaciOtpremnica(IContainer container, List<OtpremnicaDetalji> otpremnice)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Green.Lighten4).Padding(8)
                    .Text($"OTPREMNICE ({otpremnice.Count})").FontSize(12).Bold().FontColor(Colors.Green.Darken3);

                foreach (var otp in otpremnice)
                {
                    column.Item().PaddingTop(5).Border(1).BorderColor(Colors.Green.Medium).Padding(10).Column(col =>
                    {
                        // Reference na stranu 4
                        col.Item().Text(t =>
                        {
                            t.Span($"Šifra: {otp.Sifra}").FontSize(10).Bold().FontColor(Colors.Green.Darken2);
                            t.Span(" → vidi Stranu 4").FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                        });

                        col.Item().Text($"Datum: {FormatDate(otp.Datum)}").FontSize(9);
                        col.Item().Text($"Kupac: {otp.KomitentNaziv ?? "N/A"}").FontSize(9);
                        col.Item().Text($"Vozilo: {otp.Vozilo ?? "N/A"}").FontSize(9);
                        col.Item().Text($"Status: {otp.StatusNaziv ?? "N/A"}").FontSize(9);
                    });
                }
            });
        }

        private void PodaciEvidencijaRada(IContainer container, List<EvidencijaRadaDetalji> evidencije)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Blue.Lighten3).Padding(8)
                    .Text($"EVIDENCIJE RADA ({evidencije.Count})").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);

                foreach (var ev in evidencije)
                {
                    column.Item().PaddingTop(5).Border(1).BorderColor(Colors.Blue.Medium).Padding(10).Column(col =>
                    {
                        // Reference na stranu 2
                        col.Item().Text(t =>
                        {
                            t.Span($"Šifra: {ev.Sifra}").FontSize(10).Bold().FontColor(Colors.Blue.Darken2);
                            t.Span(" → vidi Stranu 2").FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                        });

                        col.Item().Text($"Datum: {FormatDate(ev.Datum)}").FontSize(9);
                        col.Item().Text($"Smena: {ev.Smena ?? 0}").FontSize(9);
                        col.Item().Text($"Smenski Izveštaj: {ev.SmenskiIzvestajSifra ?? "N/A"}").FontSize(9);
                        col.Item().Text($"Radnih Sati: {FormatDecimal(ev.BrojRadnihSati)} h").FontSize(9);
                    });
                }
            });
        }

        private void PodaciPrijemnica(IContainer container, List<PrijemnicaDetalji> prijemnice, List<PaletniListDetalji> paletniListovi)
        {
            container.Column(column =>
            {
                column.Item().Background(Colors.Red.Lighten4).Padding(8)
                    .Text($"PRIJEMNICE - NABAVKA ({prijemnice.Count})").FontSize(12).Bold().FontColor(Colors.Red.Darken3);

                foreach (var pr in prijemnice)
                {
                    column.Item().PaddingTop(5).Border(1).BorderColor(Colors.Red.Medium).Padding(10).Column(col =>
                    {
                        // Reference na stranu 3
                        col.Item().Text(t =>
                        {
                            t.Span($"Šifra: {pr.Sifra}").FontSize(10).Bold().FontColor(Colors.Red.Darken2);
                            t.Span(" → vidi Stranu 3").FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                        });

                        col.Item().Text($"Datum: {FormatDate(pr.Datum)}").FontSize(9);
                        col.Item().Text($"Dobavljač: {pr.KomitentNaziv ?? "N/A"}").FontSize(9);
                        col.Item().Text($"Otpremnica dobavljača: {pr.Otpremnica ?? "N/A"}").FontSize(9);
                        col.Item().Text($"Vozilo: {pr.Vozilo ?? "N/A"}").FontSize(9);
                        col.Item().Text($"Status: {pr.StatusNaziv ?? "N/A"}").FontSize(9);

                        // Paletni listovi
                        var palete = paletniListovi.Where(pl => pl.PrijemnicaSifra == pr.Sifra).ToList();
                        if (palete.Any())
                        {
                            col.Item().PaddingTop(5).Text($"Paletni Listovi ({palete.Count}):").FontSize(9).Italic();
                            foreach (var pl in palete.Take(5))
                            {
                                col.Item().PaddingLeft(10).Text($"• {pl.Sifra} - {pl.ArtikalNaziv} ({FormatDecimal(pl.Tezina)} kg) → Strana 3")
                                    .FontSize(8).FontColor(Colors.Red.Darken1);
                            }
                            if (palete.Count > 5)
                            {
                                col.Item().PaddingLeft(10).Text($"... i još {palete.Count - 5}").FontSize(8).Italic();
                            }
                        }
                    });
                }
            });
        }

        #endregion

        #region STRANA 2 - Dijagram Proizvodnja

        private void RadniNalogBoks(IContainer container, RadniNalogDetalji rn)
        {
            container.AlignCenter().MaxWidth(500).Column(column =>
            {
                column.Item().Background(Colors.Blue.Lighten4).Padding(8)
                    .AlignCenter().Text("RADNI NALOG").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);

                column.Item().Border(2).BorderColor(Colors.Blue.Medium)
                    .Background(Colors.Blue.Lighten5).Padding(12).Column(col =>
                    {
                        col.Item().Section($"radni_nalog_{rn.ID}");

                        col.Item().AlignCenter().Text(rn.Sifra ?? "N/A")
                            .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().AlignCenter().Text($"Kupac: {rn.KomitentNaziv ?? "N/A"}").FontSize(10);
                        col.Item().AlignCenter().Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Text($"Količina: {FormatDecimal(rn.Kolicina)} kg").FontSize(9);
                            row.RelativeItem().AlignCenter().Text($"Pakovanja: {rn.BrojPakovanja ?? 0}").FontSize(9);
                        });
                        col.Item().AlignCenter().Text($"Datum: {FormatDate(rn.Datum)}").FontSize(9);
                    });
            });
        }

        private void EvidencijeRadaDijagram(IContainer container, List<EvidencijaRadaDetalji> evidencije, List<PaletniListDetalji> paletniListovi)
        {
            container.Column(column =>
            {
                foreach (var ev in evidencije)
                {
                    // Evidencija Boks
                    column.Item().PaddingTop(10).AlignCenter().MaxWidth(500).Column(evCol =>
                    {
                        evCol.Item().Section($"evidencija_{ev.ID}");

                        evCol.Item().Background(Colors.Blue.Lighten3).Padding(6)
                            .AlignCenter().Text("EVIDENCIJA RADA").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);

                        evCol.Item().Border(2).BorderColor(Colors.Blue.Medium)
                            .Background(Colors.Blue.Lighten5).Padding(10).Column(col =>
                            {
                                col.Item().AlignCenter().Text(ev.Sifra ?? "N/A")
                                    .FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                                col.Item().AlignCenter().Text($"{FormatDate(ev.Datum)} | Smena {ev.Smena ?? 0}").FontSize(9);
                                col.Item().AlignCenter().Text($"Smenski: {ev.SmenskiIzvestajSifra ?? "N/A"}").FontSize(9);
                                col.Item().AlignCenter().Text($"Radnih sati: {FormatDecimal(ev.BrojRadnihSati)} h").FontSize(9);
                            });

                        // Paletni Listovi Gotovih Proizvoda
                        var proizvedeni = paletniListovi.Where(pl => pl.EvidencijaRadaID == ev.ID).ToList();
                        if (proizvedeni.Any())
                        {
                            evCol.Item().PaddingTop(8).Column(plCol =>
                            {
                                plCol.Item().AlignCenter().Text("PROIZVEDENI PALETNI LISTOVI")
                                    .FontSize(9).Bold().FontColor(Colors.Green.Darken2);

                                foreach (var pl in proizvedeni)
                                {
                                    plCol.Item().PaddingTop(4).AlignCenter().Border(1).BorderColor(Colors.Green.Medium)
                                        .Background(Colors.Green.Lighten5).Padding(8).Column(col =>
                                        {
                                            col.Item().Section($"paletni_izlaz_{pl.ID}");

                                            col.Item().AlignCenter()
                                                .Text($"★ {pl.Sifra}")
                                                .FontSize(10).Bold().FontColor(Colors.Green.Darken2).Underline();

                                            col.Item().AlignCenter().Text($"{pl.ArtikalNaziv ?? "N/A"}").FontSize(8);
                                            col.Item().AlignCenter().Row(row =>
                                            {
                                                row.RelativeItem().AlignCenter().Text($"Težina: {FormatDecimal(pl.Tezina)} kg").FontSize(8);
                                                if (!string.IsNullOrEmpty(pl.AmbalazaNaziv))
                                                {
                                                    row.RelativeItem().AlignCenter().Text($"Ambalaza: {pl.AmbalazaNaziv}").FontSize(8);
                                                }
                                            });

                                            if (!string.IsNullOrEmpty(pl.OtpremnicaSifra))
                                            {
                                                col.Item().AlignCenter().Text($"Otpremnica: {pl.OtpremnicaSifra}").FontSize(7).Italic();
                                            }
                                        });
                                }
                            });
                        }
                    });

                    column.Item().PaddingTop(8).AlignCenter().Text("▼").FontSize(16).FontColor(Colors.Green.Medium);
                }
            });
        }

        #endregion

        #region STRANA 3 - Dijagram Prijemnice

        private void PrijemniceDijagram(IContainer container, List<PrijemnicaDetalji> prijemnice, List<PaletniListDetalji> paletniListovi)
        {
            container.Column(column =>
            {
                foreach (var pr in prijemnice)
                {
                    column.Item().PaddingBottom(15).Column(prCol =>
                    {
                        prCol.Item().Section($"prijemnica_{pr.ID}");

                        // Prijemnica Boks
                        prCol.Item().Background(Colors.Red.Lighten4).Padding(8)
                            .Text($"PRIJEMNICA: {pr.Sifra}").FontSize(11).Bold().FontColor(Colors.Red.Darken3);

                        prCol.Item().Border(2).BorderColor(Colors.Red.Medium)
                            .Background(Colors.Red.Lighten5).Padding(10).Column(col =>
                            {
                                col.Item().Text($"Datum: {FormatDate(pr.Datum)}").FontSize(9);
                                col.Item().Text($"Dobavljač: {pr.KomitentNaziv ?? "N/A"}").FontSize(9);
                                col.Item().Text($"Otpremnica dobavljača: {pr.Otpremnica ?? "N/A"}").FontSize(9);
                                col.Item().Text($"Vozilo: {pr.Vozilo ?? "N/A"}").FontSize(9);
                            });

                        // Paletni Listovi Sirovine
                        var palete = paletniListovi.Where(pl => pl.PrijemnicaSifra == pr.Sifra).ToList();
                        if (palete.Any())
                        {
                            prCol.Item().PaddingTop(10).Column(plCol =>
                            {
                                plCol.Item().Text($"PALETNI LISTOVI SIROVINE ({palete.Count})")
                                    .FontSize(10).Bold().FontColor(Colors.Red.Darken2);

                                // Grupiši paletne listove po 2 u redu
                                for (int i = 0; i < palete.Count; i += 2)
                                {
                                    plCol.Item().PaddingTop(5).Row(row =>
                                    {
                                        // Prvi paletni list u redu
                                        var pl = palete[i];
                                        row.RelativeItem().PaddingRight(5).Border(1).BorderColor(Colors.Red.Medium)
                                            .Background(Colors.Red.Lighten5).Padding(8).Column(col =>
                                            {
                                                col.Item().Section($"paletni_ulaz_{pl.ID}");

                                                col.Item()
                                                    .Text($"▸ {pl.Sifra}")
                                                    .FontSize(9).Bold().FontColor(Colors.Red.Darken2).Underline();

                                                col.Item().Text($"{pl.ArtikalNaziv ?? "N/A"}").FontSize(8);
                                                col.Item().Text($"Težina: {FormatDecimal(pl.Tezina)} kg").FontSize(7);

                                                if (!string.IsNullOrEmpty(pl.LotDobavljaca))
                                                {
                                                    col.Item().Text($"Lot: {pl.LotDobavljaca}").FontSize(7).Italic();
                                                }

                                                if (!string.IsNullOrEmpty(pl.EvidencijaRadaSifra))
                                                {
                                                    col.Item().PaddingTop(3).Text($"Korišćen u: {pl.EvidencijaRadaSifra}")
                                                        .FontSize(7).FontColor(Colors.Blue.Darken1);
                                                }
                                            });

                                        // Drugi paletni list u redu (ako postoji)
                                        if (i + 1 < palete.Count)
                                        {
                                            var pl2 = palete[i + 1];
                                            row.RelativeItem().Border(1).BorderColor(Colors.Red.Medium)
                                                .Background(Colors.Red.Lighten5).Padding(8).Column(col =>
                                                {
                                                    col.Item().Section($"paletni_ulaz_{pl2.ID}");

                                                    col.Item()
                                                        .Text($"▸ {pl2.Sifra}")
                                                        .FontSize(9).Bold().FontColor(Colors.Red.Darken2).Underline();

                                                    col.Item().Text($"{pl2.ArtikalNaziv ?? "N/A"}").FontSize(8);
                                                    col.Item().Text($"Težina: {FormatDecimal(pl2.Tezina)} kg").FontSize(7);

                                                    if (!string.IsNullOrEmpty(pl2.LotDobavljaca))
                                                    {
                                                        col.Item().Text($"Lot: {pl2.LotDobavljaca}").FontSize(7).Italic();
                                                    }

                                                    if (!string.IsNullOrEmpty(pl2.EvidencijaRadaSifra))
                                                    {
                                                        col.Item().PaddingTop(3).Text($"Korišćen u: {pl2.EvidencijaRadaSifra}")
                                                            .FontSize(7).FontColor(Colors.Blue.Darken1);
                                                    }
                                                });
                                        }
                                    });
                                }
                            });
                        }
                    });
                }
            });
        }

        #endregion

        #region STRANA 4 - Dijagram Otpremnice

        private void OtpremniceDijagram(IContainer container, List<OtpremnicaDetalji> otpremnice, List<PaletniListDetalji> paletniListovi)
        {
            container.Column(column =>
            {
                foreach (var otp in otpremnice)
                {
                    column.Item().PaddingBottom(15).Column(otpCol =>
                    {
                        otpCol.Item().Section($"otpremnica_{otp.ID}");

                        // Otpremnica Boks
                        otpCol.Item().Background(Colors.Green.Lighten4).Padding(8)
                            .Text($"OTPREMNICA: {otp.Sifra}").FontSize(11).Bold().FontColor(Colors.Green.Darken3);

                        otpCol.Item().Border(2).BorderColor(Colors.Green.Medium)
                            .Background(Colors.Green.Lighten5).Padding(10).Column(col =>
                            {
                                col.Item().Text($"Datum: {FormatDate(otp.Datum)}").FontSize(9);
                                col.Item().Text($"Kupac: {otp.KomitentNaziv ?? "N/A"}").FontSize(9);
                                col.Item().Text($"Vozilo: {otp.Vozilo ?? "N/A"}").FontSize(9);
                            });

                        // Paletni Listovi Gotove Robe
                        var palete = paletniListovi.Where(pl => pl.OtpremnicaSifra == otp.Sifra).ToList();
                        if (palete.Any())
                        {
                            otpCol.Item().PaddingTop(10).Column(plCol =>
                            {
                                plCol.Item().Text($"PALETNI LISTOVI GOTOVE ROBE ({palete.Count})")
                                    .FontSize(10).Bold().FontColor(Colors.Green.Darken2);

                                // Grupiši paletne listove po 2 u redu
                                for (int i = 0; i < palete.Count; i += 2)
                                {
                                    plCol.Item().PaddingTop(5).Row(row =>
                                    {
                                        // Prvi paletni list u redu
                                        var pl = palete[i];
                                        row.RelativeItem().PaddingRight(5).Border(1).BorderColor(Colors.Green.Medium)
                                            .Background(Colors.Green.Lighten5).Padding(8).Column(col =>
                                            {
                                                col.Item().Section($"paletni_izlaz_{pl.ID}");

                                                col.Item()
                                                    .Text($"★ {pl.Sifra}")
                                                    .FontSize(9).Bold().FontColor(Colors.Green.Darken2).Underline();

                                                col.Item().Text($"{pl.ArtikalNaziv ?? "N/A"}").FontSize(8);
                                                col.Item().Text($"Težina: {FormatDecimal(pl.Tezina)} kg").FontSize(7);

                                                if (!string.IsNullOrEmpty(pl.AmbalazaNaziv))
                                                {
                                                    col.Item().Text($"Ambalaza: {pl.AmbalazaNaziv}").FontSize(7);
                                                }

                                                if (!string.IsNullOrEmpty(pl.PakovanjeNaziv))
                                                {
                                                    col.Item().Text($"Pakovanje: {pl.PakovanjeNaziv}").FontSize(7);
                                                }

                                                if (!string.IsNullOrEmpty(pl.EvidencijaRadaSifra))
                                                {
                                                    col.Item().PaddingTop(3).Text($"Proizveden u: {pl.EvidencijaRadaSifra}")
                                                        .FontSize(7).FontColor(Colors.Blue.Darken1);
                                                }

                                                if (!string.IsNullOrEmpty(pl.SmenskiIzvestajSifra))
                                                {
                                                    col.Item().Text($"Smena: {pl.SmenskiIzvestajSifra}")
                                                        .FontSize(7).FontColor(Colors.Blue.Darken1);
                                                }
                                            });

                                        // Drugi paletni list u redu (ako postoji)
                                        if (i + 1 < palete.Count)
                                        {
                                            var pl2 = palete[i + 1];
                                            row.RelativeItem().Border(1).BorderColor(Colors.Green.Medium)
                                                .Background(Colors.Green.Lighten5).Padding(8).Column(col =>
                                                {
                                                    col.Item().Section($"paletni_izlaz_{pl2.ID}");

                                                    col.Item()
                                                        .Text($"★ {pl2.Sifra}")
                                                        .FontSize(9).Bold().FontColor(Colors.Green.Darken2).Underline();

                                                    col.Item().Text($"{pl2.ArtikalNaziv ?? "N/A"}").FontSize(8);
                                                    col.Item().Text($"Težina: {FormatDecimal(pl2.Tezina)} kg").FontSize(7);

                                                    if (!string.IsNullOrEmpty(pl2.AmbalazaNaziv))
                                                    {
                                                        col.Item().Text($"Ambalaza: {pl2.AmbalazaNaziv}").FontSize(7);
                                                    }

                                                    if (!string.IsNullOrEmpty(pl2.PakovanjeNaziv))
                                                    {
                                                        col.Item().Text($"Pakovanje: {pl2.PakovanjeNaziv}").FontSize(7);
                                                    }

                                                    if (!string.IsNullOrEmpty(pl2.EvidencijaRadaSifra))
                                                    {
                                                        col.Item().PaddingTop(3).Text($"Proizveden u: {pl2.EvidencijaRadaSifra}")
                                                            .FontSize(7).FontColor(Colors.Blue.Darken1);
                                                    }

                                                    if (!string.IsNullOrEmpty(pl2.SmenskiIzvestajSifra))
                                                    {
                                                        col.Item().Text($"Smena: {pl2.SmenskiIzvestajSifra}")
                                                            .FontSize(7).FontColor(Colors.Blue.Darken1);
                                                    }
                                                });
                                        }
                                    });
                                }
                            });
                        }
                    });
                }
            });
        }

        #endregion

        #region Utility Methods

        private string FormatDate(DateTime? date)
        {
            return date?.ToString("dd.MM.yyyy", SrpskiFormat) ?? "N/A";
        }

        private string FormatDecimal(decimal? value)
        {
            return value?.ToString("N2", SrpskiFormat) ?? "0,00";
        }

        #endregion
    }
}
