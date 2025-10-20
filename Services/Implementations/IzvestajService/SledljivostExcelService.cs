using ClosedXML.Excel;
using FruitSysWeb.Models.Sledljivost;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class SledljivostExcelService
    {
        /// <summary>
        /// Generiše UPSTREAM Excel sa master tabelom i filterima
        /// </summary>
        public byte[] GenerisiUpstreamExcel(SledljivostModel sledljivost)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Sledljivost - UPSTREAM");

            // Kreiraj master tabelu sa svim podacima
            KreirajUpstreamMasterTabelu(sheet, sledljivost);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Generiše DOWNSTREAM Excel sa master tabelom i filterima
        /// </summary>
        public byte[] GenerisiDownstreamExcel(SledljivostModel sledljivost)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Sledljivost - DOWNSTREAM");

            // Kreiraj master tabelu sa svim podacima
            KreirajDownstreamMasterTabelu(sheet, sledljivost);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        #region UPSTREAM Master Tabela

        private void KreirajUpstreamMasterTabelu(IXLWorksheet sheet, SledljivostModel sledljivost)
        {
            // Info header
            sheet.Cell("A1").Value = "UPSTREAM SLEDLJIVOST";
            sheet.Cell("A1").Style.Font.Bold = true;
            sheet.Cell("A1").Style.Font.FontSize = 16;
            sheet.Cell("A1").Style.Fill.BackgroundColor = XLColor.LightBlue;
            sheet.Range("A1:N1").Merge();

            sheet.Cell("A2").Value = $"Radni Nalog: {sledljivost.RadniNalog?.Sifra ?? "N/A"}";
            sheet.Cell("A2").Style.Font.Bold = true;
            sheet.Range("A2:N2").Merge();

            sheet.Cell("A3").Value = $"Datum: {DateTime.Now:dd.MM.yyyy HH:mm}";
            sheet.Range("A3:N3").Merge();

            // Tabela počinje od reda 5
            int headerRow = 5;

            // HEADERI
            sheet.Cell(headerRow, 1).Value = "Faza";
            sheet.Cell(headerRow, 2).Value = "Tip Dokumenta";
            sheet.Cell(headerRow, 3).Value = "Šifra Dokumenta";
            sheet.Cell(headerRow, 4).Value = "Datum";
            sheet.Cell(headerRow, 5).Value = "Komitent";
            sheet.Cell(headerRow, 6).Value = "Paletni List";
            sheet.Cell(headerRow, 7).Value = "Artikal";
            sheet.Cell(headerRow, 8).Value = "Težina";
            sheet.Cell(headerRow, 9).Value = "Evidencija Rada";
            sheet.Cell(headerRow, 10).Value = "Smena";
            sheet.Cell(headerRow, 11).Value = "Radni Nalog";
            sheet.Cell(headerRow, 12).Value = "Otpremnica";
            sheet.Cell(headerRow, 13).Value = "Vozilo";
            sheet.Cell(headerRow, 14).Value = "Status";

            // Stilizuj header
            var headerRange = sheet.Range(headerRow, 1, headerRow, 14);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.Gray;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            int row = headerRow + 1;

            // FAZA 1: NABAVKA - Prijemnice i njihovi paletni listovi
            if (sledljivost.Prijemnice?.Any() == true)
            {
                foreach (var prijemnica in sledljivost.Prijemnice)
                {
                    // Red za Prijemnicu
                    sheet.Cell(row, 1).Value = "1. NABAVKA";
                    sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightPink;
                    sheet.Cell(row, 2).Value = "Prijemnica";
                    sheet.Cell(row, 3).Value = prijemnica.Sifra;
                    sheet.Cell(row, 4).Value = prijemnica.Datum?.ToString("dd.MM.yyyy");
                    sheet.Cell(row, 5).Value = prijemnica.KomitentNaziv;
                    sheet.Cell(row, 13).Value = prijemnica.Vozilo;
                    sheet.Cell(row, 14).Value = prijemnica.StatusNaziv;
                    row++;

                    // Paletni listovi iz ove prijemnice
                    var paleteIzPrijemnice = sledljivost.PaletniListoviUlaz?
                        .Where(pl => pl.PrijemnicaSifra == prijemnica.Sifra)
                        .ToList();

                    if (paleteIzPrijemnice?.Any() == true)
                    {
                        foreach (var pl in paleteIzPrijemnice)
                        {
                            sheet.Cell(row, 1).Value = "1. NABAVKA";
                            sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightPink;
                            sheet.Cell(row, 2).Value = "  → Paletni List";
                            sheet.Cell(row, 3).Value = prijemnica.Sifra;
                            sheet.Cell(row, 6).Value = pl.Sifra;
                            sheet.Cell(row, 7).Value = pl.ArtikalNaziv;
                            sheet.Cell(row, 8).Value = pl.Tezina;
                            sheet.Cell(row, 14).Value = pl.StatusNaziv;
                            row++;
                        }
                    }
                }
            }

            // FAZA 2: PROIZVODNJA - Evidencije Rada
            if (sledljivost.EvidencijeRada?.Any() == true)
            {
                foreach (var er in sledljivost.EvidencijeRada)
                {
                    sheet.Cell(row, 1).Value = "2. PROIZVODNJA";
                    sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightCyan;
                    sheet.Cell(row, 2).Value = "Evidencija Rada";
                    sheet.Cell(row, 3).Value = er.Sifra;
                    sheet.Cell(row, 4).Value = er.Datum?.ToString("dd.MM.yyyy");
                    sheet.Cell(row, 9).Value = er.Sifra;
                    sheet.Cell(row, 10).Value = er.Smena;
                    sheet.Cell(row, 11).Value = sledljivost.RadniNalog?.Sifra;
                    row++;

                    // Utrošeni paletni listovi
                    if (er.UtroseniPaletniListoviIDs?.Any() == true)
                    {
                        var utroseniPL = sledljivost.PaletniListoviUlaz?
                            .Where(pl => er.UtroseniPaletniListoviIDs.Contains(pl.ID))
                            .ToList();

                        if (utroseniPL?.Any() == true)
                        {
                            foreach (var pl in utroseniPL)
                            {
                                sheet.Cell(row, 1).Value = "2. PROIZVODNJA";
                                sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightCyan;
                                sheet.Cell(row, 2).Value = "  → Utrošeno";
                                sheet.Cell(row, 3).Value = er.Sifra;
                                sheet.Cell(row, 6).Value = pl.Sifra;
                                sheet.Cell(row, 7).Value = pl.ArtikalNaziv;
                                sheet.Cell(row, 8).Value = pl.Tezina;
                                row++;
                            }
                        }
                    }

                    // Proizvedeni paletni listovi (iz PaletniListoviIzlaz)
                    var proizvedeniPL = sledljivost.PaletniListoviIzlaz?
                        .Where(pl => pl.EvidencijaRadaID == er.ID)
                        .ToList();

                    if (proizvedeniPL?.Any() == true)
                    {
                        foreach (var pl in proizvedeniPL)
                        {
                            sheet.Cell(row, 1).Value = "2. PROIZVODNJA";
                            sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightCyan;
                            sheet.Cell(row, 2).Value = "  → Proizvod";
                            sheet.Cell(row, 3).Value = er.Sifra;
                            sheet.Cell(row, 6).Value = pl.Sifra;
                            sheet.Cell(row, 7).Value = pl.ArtikalNaziv;
                            sheet.Cell(row, 8).Value = pl.Tezina;
                            sheet.Cell(row, 11).Value = pl.RadniNalogSifra;
                            row++;
                        }
                    }
                }
            }

            // FAZA 3: PRODAJA - Otpremnice
            if (sledljivost.Otpremnice?.Any() == true)
            {
                foreach (var otpremnica in sledljivost.Otpremnice)
                {
                    sheet.Cell(row, 1).Value = "3. PRODAJA";
                    sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    sheet.Cell(row, 2).Value = "Otpremnica";
                    sheet.Cell(row, 3).Value = otpremnica.Sifra;
                    sheet.Cell(row, 4).Value = otpremnica.Datum?.ToString("dd.MM.yyyy");
                    sheet.Cell(row, 5).Value = otpremnica.KomitentNaziv;
                    sheet.Cell(row, 11).Value = sledljivost.RadniNalog?.Sifra;
                    sheet.Cell(row, 12).Value = otpremnica.Sifra;
                    sheet.Cell(row, 13).Value = otpremnica.Vozilo;
                    sheet.Cell(row, 14).Value = otpremnica.StatusNaziv;
                    row++;

                    // Paletni listovi u otpremnici
                    var paleteUOtpremnici = sledljivost.PaletniListoviIzlaz?
                        .Where(pl => pl.OtpremnicaStavkaID.HasValue)
                        .ToList();

                    if (paleteUOtpremnici?.Any() == true)
                    {
                        foreach (var pl in paleteUOtpremnici)
                        {
                            sheet.Cell(row, 1).Value = "3. PRODAJA";
                            sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
                            sheet.Cell(row, 2).Value = "  → Paletni List";
                            sheet.Cell(row, 3).Value = otpremnica.Sifra;
                            sheet.Cell(row, 6).Value = pl.Sifra;
                            sheet.Cell(row, 7).Value = pl.ArtikalNaziv;
                            sheet.Cell(row, 8).Value = pl.Tezina;
                            sheet.Cell(row, 12).Value = otpremnica.Sifra;
                            row++;
                        }
                    }
                }
            }

            // Kreiraj Excel Tabelu sa AutoFilter
            var dataRange = sheet.Range(headerRow, 1, row - 1, 14);
            var table = dataRange.CreateTable("SledljivostUpstream");
            table.Theme = XLTableTheme.TableStyleMedium2;

            // Auto-fit kolone
            sheet.ColumnsUsed().AdjustToContents();

            // Zamrzni header redove
            sheet.SheetView.FreezeRows(headerRow);
        }

        #endregion

        #region DOWNSTREAM Master Tabela

        private void KreirajDownstreamMasterTabelu(IXLWorksheet sheet, SledljivostModel sledljivost)
        {
            var glavniPL = sledljivost.PaletniListoviIzlaz?.FirstOrDefault();

            // Info header
            sheet.Cell("A1").Value = "DOWNSTREAM SLEDLJIVOST";
            sheet.Cell("A1").Style.Font.Bold = true;
            sheet.Cell("A1").Style.Font.FontSize = 16;
            sheet.Cell("A1").Style.Fill.BackgroundColor = XLColor.LightGreen;
            sheet.Range("A1:N1").Merge();

            sheet.Cell("A2").Value = $"Paletni List: {glavniPL?.Sifra ?? "N/A"}";
            sheet.Cell("A2").Style.Font.Bold = true;
            sheet.Range("A2:N2").Merge();

            sheet.Cell("A3").Value = $"Artikal: {glavniPL?.ArtikalNaziv ?? "N/A"} ({glavniPL?.Tezina:F2} kg)";
            sheet.Range("A3:N3").Merge();

            sheet.Cell("A4").Value = $"Datum: {DateTime.Now:dd.MM.yyyy HH:mm}";
            sheet.Range("A4:N4").Merge();

            // Tabela počinje od reda 6
            int headerRow = 6;

            // HEADERI
            sheet.Cell(headerRow, 1).Value = "Faza";
            sheet.Cell(headerRow, 2).Value = "Tip Dokumenta";
            sheet.Cell(headerRow, 3).Value = "Šifra Dokumenta";
            sheet.Cell(headerRow, 4).Value = "Datum";
            sheet.Cell(headerRow, 5).Value = "Komitent";
            sheet.Cell(headerRow, 6).Value = "Paletni List";
            sheet.Cell(headerRow, 7).Value = "Artikal";
            sheet.Cell(headerRow, 8).Value = "Težina";
            sheet.Cell(headerRow, 9).Value = "Evidencija Rada";
            sheet.Cell(headerRow, 10).Value = "Smena";
            sheet.Cell(headerRow, 11).Value = "Radni Nalog";
            sheet.Cell(headerRow, 12).Value = "Prijemnica";
            sheet.Cell(headerRow, 13).Value = "Vozilo";
            sheet.Cell(headerRow, 14).Value = "Status";

            // Stilizuj header
            var headerRange = sheet.Range(headerRow, 1, headerRow, 14);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.Gray;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            int row = headerRow + 1;

            // FAZA 1: GDE JE PRODAT
            if (glavniPL != null)
            {
                sheet.Cell(row, 1).Value = "1. GDE JE PRODAT";
                sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
                sheet.Cell(row, 2).Value = "Glavni Proizvod";
                sheet.Cell(row, 6).Value = glavniPL.Sifra;
                sheet.Cell(row, 7).Value = glavniPL.ArtikalNaziv;
                sheet.Cell(row, 8).Value = glavniPL.Tezina;
                sheet.Cell(row, 11).Value = glavniPL.RadniNalogSifra;
                sheet.Cell(row, 14).Value = glavniPL.StatusNaziv;
                row++;
            }

            if (sledljivost.RadniNalog != null)
            {
                sheet.Cell(row, 1).Value = "1. GDE JE PRODAT";
                sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
                sheet.Cell(row, 2).Value = "Radni Nalog";
                sheet.Cell(row, 3).Value = sledljivost.RadniNalog.Sifra;
                sheet.Cell(row, 4).Value = sledljivost.RadniNalog.Datum?.ToString("dd.MM.yyyy");
                sheet.Cell(row, 5).Value = sledljivost.RadniNalog.KomitentNaziv;
                sheet.Cell(row, 11).Value = sledljivost.RadniNalog.Sifra;
                sheet.Cell(row, 14).Value = sledljivost.RadniNalog.StatusNaziv;
                row++;
            }

            if (sledljivost.Otpremnice?.Any() == true)
            {
                foreach (var otpremnica in sledljivost.Otpremnice)
                {
                    sheet.Cell(row, 1).Value = "1. GDE JE PRODAT";
                    sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    sheet.Cell(row, 2).Value = "Otpremnica";
                    sheet.Cell(row, 3).Value = otpremnica.Sifra;
                    sheet.Cell(row, 4).Value = otpremnica.Datum?.ToString("dd.MM.yyyy");
                    sheet.Cell(row, 5).Value = otpremnica.KomitentNaziv;
                    sheet.Cell(row, 11).Value = sledljivost.RadniNalog?.Sifra;
                    sheet.Cell(row, 13).Value = otpremnica.Vozilo;
                    sheet.Cell(row, 14).Value = otpremnica.StatusNaziv;
                    row++;
                }
            }

            // FAZA 2: KAKO JE PROIZVEDEN
            if (sledljivost.EvidencijeRada?.Any() == true)
            {
                foreach (var er in sledljivost.EvidencijeRada)
                {
                    sheet.Cell(row, 1).Value = "2. KAKO JE PROIZVEDEN";
                    sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightCyan;
                    sheet.Cell(row, 2).Value = "Evidencija Rada";
                    sheet.Cell(row, 3).Value = er.Sifra;
                    sheet.Cell(row, 4).Value = er.Datum?.ToString("dd.MM.yyyy");
                    sheet.Cell(row, 9).Value = er.Sifra;
                    sheet.Cell(row, 10).Value = er.Smena;
                    row++;

                    // Utrošeni paletni listovi (sirovine)
                    if (er.UtroseniPaletniListoviIDs?.Any() == true)
                    {
                        var utroseniPL = sledljivost.PaletniListoviUlaz?
                            .Where(pl => er.UtroseniPaletniListoviIDs.Contains(pl.ID))
                            .ToList();

                        if (utroseniPL?.Any() == true)
                        {
                            foreach (var pl in utroseniPL)
                            {
                                sheet.Cell(row, 1).Value = "2. KAKO JE PROIZVEDEN";
                                sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightCyan;
                                sheet.Cell(row, 2).Value = "  → Utrošena Sirovina";
                                sheet.Cell(row, 3).Value = er.Sifra;
                                sheet.Cell(row, 6).Value = pl.Sifra;
                                sheet.Cell(row, 7).Value = pl.ArtikalNaziv;
                                sheet.Cell(row, 8).Value = pl.Tezina;
                                sheet.Cell(row, 12).Value = pl.PrijemnicaSifra;
                                row++;
                            }
                        }
                    }
                }
            }

            // FAZA 3: ODAKLE DOLAZI (SIROVINE)
            if (sledljivost.Prijemnice?.Any() == true)
            {
                foreach (var prijemnica in sledljivost.Prijemnice)
                {
                    sheet.Cell(row, 1).Value = "3. ODAKLE DOLAZI";
                    sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightPink;
                    sheet.Cell(row, 2).Value = "Prijemnica Sirovine";
                    sheet.Cell(row, 3).Value = prijemnica.Sifra;
                    sheet.Cell(row, 4).Value = prijemnica.Datum?.ToString("dd.MM.yyyy");
                    sheet.Cell(row, 5).Value = prijemnica.KomitentNaziv;
                    sheet.Cell(row, 12).Value = prijemnica.Sifra;
                    sheet.Cell(row, 13).Value = prijemnica.Vozilo;
                    sheet.Cell(row, 14).Value = prijemnica.StatusNaziv;
                    row++;

                    // Paletni listovi sirovine iz prijemnice
                    var paleteIzPrijemnice = sledljivost.PaletniListoviUlaz?
                        .Where(pl => pl.PrijemnicaSifra == prijemnica.Sifra)
                        .ToList();

                    if (paleteIzPrijemnice?.Any() == true)
                    {
                        foreach (var pl in paleteIzPrijemnice)
                        {
                            sheet.Cell(row, 1).Value = "3. ODAKLE DOLAZI";
                            sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightPink;
                            sheet.Cell(row, 2).Value = "  → Paletni List Sirovine";
                            sheet.Cell(row, 3).Value = prijemnica.Sifra;
                            sheet.Cell(row, 6).Value = pl.Sifra;
                            sheet.Cell(row, 7).Value = pl.ArtikalNaziv;
                            sheet.Cell(row, 8).Value = pl.Tezina;
                            sheet.Cell(row, 12).Value = prijemnica.Sifra;
                            row++;
                        }
                    }
                }
            }

            // Kreiraj Excel Tabelu sa AutoFilter
            var dataRange = sheet.Range(headerRow, 1, row - 1, 14);
            var table = dataRange.CreateTable("SledljivostDownstream");
            table.Theme = XLTableTheme.TableStyleMedium2;

            // Auto-fit kolone
            sheet.ColumnsUsed().AdjustToContents();

            // Zamrzni header redove
            sheet.SheetView.FreezeRows(headerRow);
        }

        #endregion
    }
}
