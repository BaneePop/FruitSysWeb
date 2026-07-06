using FruitSysWeb.Models.IzvodDokumenata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.IO.Compression;
using ZXing;
using ZXing.Common;

namespace FruitSysWeb.Services.Implementations.ExportService
{
    public class PaletniListPdfService
    {
        private static readonly CultureInfo Sr = new("sr-Latn-RS");

        private const string OdettaAdresa = "ODETTA DOO, Kralja Dragutina 5, 7/31, 15000 Šabac, Srbija";
        private const string OdettaOgranak = "Ogranak Hladnjača Riđake: Šabački put 18A, 15224 Riđake";

        public byte[] Generisi(IzvodPaletniListDetalji model)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(1.2f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                    page.Content().Element(c => BuildContent(c, model));
                });
            }).GeneratePdf();
        }

        private void BuildContent(IContainer container, IzvodPaletniListDetalji model)
        {
            var barcodeBytes = GenerisiBarcode(model.Sifra);

            container.Column(col =>
            {
                col.Spacing(0);

                // ── Firma header ──
                col.Item().PaddingBottom(4).Column(c =>
                {
                    c.Item().Text(OdettaAdresa).FontSize(8);
                    c.Item().Text(OdettaOgranak).FontSize(8);
                });

                col.Item().LineHorizontal(1).LineColor(Colors.Black);
                col.Item().PaddingBottom(4);

                // ── Info tabelica levo + barcode desno ──
                col.Item().PaddingBottom(4).Row(row =>
                {
                    // Leva kolona: DATUM / PALETNI LIST / AMBALAZA / BRUTO
                    row.RelativeItem().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(75);
                            cols.RelativeColumn();
                        });

                        void InfoRed(string naziv, string vrednost)
                        {
                            table.Cell().Text(naziv).FontSize(9).Bold();
                            table.Cell().Text(vrednost).FontSize(9);
                        }

                        // Datum sa brojem ambalaže u zagradama
                        var datumTekst = model.DatumFormatted;
                        // Ako ima broj ambalaže, dodaj u zagradama kao u originalu (BrojAmbalaze nije u modelu, koristimo stavke count)
                        InfoRed("DATUM:", datumTekst);
                        InfoRed("PALETNI LIST:", model.Sifra);
                        InfoRed("AMBALAZA:", model.AmbalazaNaziv);
                        InfoRed("BRUTO:", model.BrutoTezina.ToString("N2", Sr));
                    });

                    // Desna kolona: barcode
                    if (barcodeBytes != null)
                    {
                        row.ConstantItem(110).AlignRight().Column(c =>
                        {
                            c.Item().Image(barcodeBytes).FitArea();
                            c.Item().AlignCenter().Text(model.Sifra).FontSize(8);
                        });
                    }
                });

                col.Item().LineHorizontal(1).LineColor(Colors.Black);
                col.Item().PaddingBottom(6);

                // ── NETO velika cifra ──
                col.Item().Row(row =>
                {
                    row.ConstantItem(55).AlignBottom().Text("NETO:").FontSize(12).Bold();
                    row.RelativeItem().Text(model.NetoTezina.ToString("N2", Sr)).FontSize(42).Bold();
                    row.ConstantItem(25).AlignBottom().PaddingBottom(6).Text("kg").FontSize(14).Bold();
                });

                col.Item().LineHorizontal(1).LineColor(Colors.Black);
                col.Item().PaddingBottom(4);

                // ── Komitent desno ──
                if (!string.IsNullOrEmpty(model.Komitent))
                {
                    col.Item().AlignRight().Text(model.Komitent).FontSize(11).Bold();
                }

                // ── Artikal veliki naziv ──
                col.Item().PaddingBottom(6).Text(model.ArtikalNaziv).FontSize(24).Bold();

                // ── Prosečna težina + Referentni broj ──
                col.Item().PaddingBottom(4).Row(row =>
                {
                    if (model.ProsecnaTezina > 0)
                    {
                        row.RelativeItem().Text(t =>
                        {
                            t.Span("Prosečna težina voća u gajbicama:").FontSize(8);
                            t.Span($"   {model.ProsecnaTezinaFormatted} kg").FontSize(8).Bold();
                        });
                    }
                    else
                    {
                        row.RelativeItem();
                    }

                    // Referentni broj (Prijemnica za nabavku, Radni nalog za gotovu robu)
                    var refBroj = model.JeNabavka ? model.OtpremnicaSifra : model.RadniNalogSifra;
                    if (!string.IsNullOrEmpty(refBroj))
                        row.ConstantItem(100).AlignRight().Text(refBroj).FontSize(9).Bold();
                });

                col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                col.Item().PaddingBottom(2);

                // ── Stavke tabela ──
                if (model.Stavke.Count > 0)
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(5);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(1.5f);
                            cols.RelativeColumn(1.2f);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Grey.Lighten2).Border(0.5f).BorderColor(Colors.Grey.Medium)
                                .Padding(3).AlignCenter().Text("ARTIKAL").FontSize(8).Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Border(0.5f).BorderColor(Colors.Grey.Medium)
                                .Padding(3).AlignCenter().Text("JM").FontSize(8).Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Border(0.5f).BorderColor(Colors.Grey.Medium)
                                .Padding(3).AlignCenter().Text("KOL").FontSize(8).Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Border(0.5f).BorderColor(Colors.Grey.Medium)
                                .Padding(3).AlignCenter().Text("%").FontSize(8).Bold();
                        });

                        foreach (var s in model.Stavke)
                        {
                            table.Cell().Border(0.3f).BorderColor(Colors.Grey.Lighten2).Padding(2)
                                .Text(s.Artikal).FontSize(8);
                            table.Cell().Border(0.3f).BorderColor(Colors.Grey.Lighten2).Padding(2)
                                .AlignCenter().Text(s.JM).FontSize(8);
                            table.Cell().Border(0.3f).BorderColor(Colors.Grey.Lighten2).Padding(2)
                                .AlignRight().Text(s.KolicinaFormatted).FontSize(8);
                            table.Cell().Border(0.3f).BorderColor(Colors.Grey.Lighten2).Padding(2)
                                .AlignRight().Text(s.Procenat > 0 ? s.ProcenatFormatted : "0.0").FontSize(8);
                        }
                    });

                    col.Item().PaddingBottom(4);
                }

                // ── Dno: Otpremnica/LOT ili Dobijen od ──
                if (model.JeNabavka)
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("OTPREMNICA:").FontSize(8).Bold();
                            c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Medium)
                                .Text(model.OtpremnicaSifra).FontSize(8);
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("LOT:").FontSize(8).Bold();
                            c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Medium)
                                .Text(model.LotNaloga).FontSize(8);
                        });
                    });
                }
                else
                {
                    // Gotova roba: Otpremnica/LOT + Dobijen od
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("OTPREMNICA:").FontSize(8).Bold();
                            c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Medium)
                                .Text(model.RadniNalogSifra).FontSize(8);
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("LOT:").FontSize(8).Bold();
                            c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Medium)
                                .Text(string.Empty).FontSize(8);
                        });
                    });

                    if (model.DobijenOd.Count > 0)
                    {
                        col.Item().PaddingTop(4).Row(row =>
                        {
                            row.ConstantItem(65).Text("DOBIJEN OD:").FontSize(8).Bold();
                            row.RelativeItem().Text(string.Join(", ", model.DobijenOd)).FontSize(8);
                        });
                    }
                }

                col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
            });
        }

        private static byte[]? GenerisiBarcode(string tekst)
        {
            try
            {
                var writer = new BarcodeWriterPixelData
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Width = 320,
                        Height = 60,
                        Margin = 4,
                        PureBarcode = true
                    }
                };

                var pixelData = writer.Write(tekst);
                return PixelDataToPng(pixelData.Pixels, pixelData.Width, pixelData.Height);
            }
            catch
            {
                return null;
            }
        }

        private static byte[] PixelDataToPng(byte[] pixels, int width, int height)
        {
            using var ms = new MemoryStream();

            ms.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

            WriteChunk(ms, "IHDR", w =>
            {
                w.Write(ToBigEndian(width));
                w.Write(ToBigEndian(height));
                w.Write((byte)8);
                w.Write((byte)2);
                w.Write((byte)0);
                w.Write((byte)0);
                w.Write((byte)0);
            });

            var scanlines = new byte[height * (1 + width * 3)];
            for (int y = 0; y < height; y++)
            {
                int sl = y * (1 + width * 3);
                scanlines[sl] = 0;
                for (int x = 0; x < width; x++)
                {
                    int px = (y * width + x) * 4;
                    scanlines[sl + 1 + x * 3 + 0] = pixels[px + 2];
                    scanlines[sl + 1 + x * 3 + 1] = pixels[px + 1];
                    scanlines[sl + 1 + x * 3 + 2] = pixels[px + 0];
                }
            }

            byte[] compressed;
            using (var deflateMs = new MemoryStream())
            {
                deflateMs.WriteByte(0x78);
                deflateMs.WriteByte(0x9C);
                using (var deflate = new DeflateStream(deflateMs, CompressionLevel.Optimal, leaveOpen: true))
                    deflate.Write(scanlines, 0, scanlines.Length);
                deflateMs.Write(ToBigEndian((int)Adler32(scanlines)));
                compressed = deflateMs.ToArray();
            }

            WriteChunk(ms, "IDAT", w => w.Write(compressed));
            WriteChunk(ms, "IEND", _ => { });

            return ms.ToArray();
        }

        private static void WriteChunk(Stream ms, string type, Action<BinaryWriter> dataWriter)
        {
            using var dataMs = new MemoryStream();
            using (var bw = new BinaryWriter(dataMs, System.Text.Encoding.ASCII, leaveOpen: true))
                dataWriter(bw);
            var data = dataMs.ToArray();
            var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);

            var crcInput = new byte[4 + data.Length];
            typeBytes.CopyTo(crcInput, 0);
            data.CopyTo(crcInput, 4);

            using var bwMain = new BinaryWriter(ms, System.Text.Encoding.ASCII, leaveOpen: true);
            bwMain.Write(ToBigEndian(data.Length));
            bwMain.Write(typeBytes);
            bwMain.Write(data);
            bwMain.Write(ToBigEndian((int)Crc32(crcInput)));
        }

        private static byte[] ToBigEndian(int value)
        {
            var b = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }

        private static uint Crc32(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte b in data)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
            }
            return crc ^ 0xFFFFFFFF;
        }

        private static uint Adler32(byte[] data)
        {
            uint a = 1, b = 0;
            foreach (byte bt in data) { a = (a + bt) % 65521; b = (b + a) % 65521; }
            return (b << 16) | a;
        }
    }
}
