-- =====================================================
-- FruitSysWeb - Database Index Optimization Script
-- Generated: 2025-10-16
-- Purpose: Add missing indexes for performance optimization
-- =====================================================

USE fruitsysdb_v2;

-- =====================================================
-- Index 1: RadniNalog - Composite Index for UgovorService
-- Usage: UgovorService subqueries (N+1 optimization)
-- Priority: CRITICAL
-- Expected Improvement: 50-60% faster subquery execution
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_RadniNalog_UgovorProdaja_Status
ON RadniNalog(UgovorProdajaID, DokumentStatus, Kolicina);

-- Alternative with INCLUDE (MySQL 8.0.13+):
-- CREATE INDEX idx_RadniNalog_UgovorProdaja_Status_v2
-- ON RadniNalog(UgovorProdajaID, DokumentStatus)
-- INCLUDE (Kolicina, ID, Aktivno);

-- =====================================================
-- Index 2: EvidencijaRada - Multi-column Index
-- Usage: PreradaService for smenske izvještaje
-- Priority: HIGH
-- Expected Improvement: 30-40% faster reporting queries
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_EvidencijaRada_SmenskiIzvestaj_RadniNalog
ON EvidencijaRada(SmenskiIzvestajID, RadniNalogID, DokumentStatus);

-- =====================================================
-- Index 3: UgovorProdajaStavka - Covering Index
-- Usage: UgovorService for loading contract items
-- Priority: HIGH
-- Expected Improvement: 40-50% faster contract loading
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_UgovorProdajaStavka_UgovorID
ON UgovorProdajaStavka(UgovorProdajaID, ArtikalID);

-- =====================================================
-- Index 4: Artikal - Filtering Index
-- Usage: Multiple services for MagacinID filtering
-- Priority: MEDIUM
-- Expected Improvement: 20-30% faster article lookups
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_Artikal_MagacinID_Aktivno
ON Artikal(MagacinID, Aktivno);

-- =====================================================
-- Index 5: RadniNalog - ArtikalInstanca Index
-- Usage: MagacinLagerService for otvoreni nalozi
-- Priority: HIGH
-- Expected Improvement: 25-35% faster inventory queries
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_RadniNalog_ArtikalInstanca_Status
ON RadniNalog(ArtikalInstancaID, DokumentStatus, Kolicina);

-- =====================================================
-- Index 6: ArtikalInstanca - Artikal Lookup
-- Usage: Multiple services for linking instances to articles
-- Priority: MEDIUM
-- Expected Improvement: 15-25% faster instance lookups
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_ArtikalInstanca_ArtikalID
ON ArtikalInstanca(ArtikalID);

-- =====================================================
-- Index 7: Faktura - Date and Status Index
-- Usage: Dashboard and financial services
-- Priority: HIGH
-- Expected Improvement: 30-40% faster dashboard queries
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_Faktura_Datum_Status_Aktivno
ON Faktura(Datum, DokumentStatus, Aktivno);

-- =====================================================
-- Index 8: OtkupniList - Date and Status Index
-- Usage: Dashboard and financial services
-- Priority: HIGH
-- Expected Improvement: 30-40% faster dashboard queries
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_OtkupniList_Datum_Status_Aktivno
ON OtkupniList(Datum, DokumentStatus, Aktivno);

-- =====================================================
-- Index 9: Prijemnica - Date and Status Index
-- Usage: Dashboard and UlazIzlaz services
-- Priority: HIGH
-- Expected Improvement: 30-40% faster document queries
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_Prijemnica_Datum_Status_Aktivno
ON Prijemnica(Datum, DokumentStatus, Aktivno);

-- =====================================================
-- Index 10: Otpremnica - Date and Status Index
-- Usage: Dashboard and UlazIzlaz services
-- Priority: HIGH
-- Expected Improvement: 30-40% faster document queries
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_Otpremnica_Datum_Status_Aktivno
ON Otpremnica(Datum, DokumentStatus, Aktivno);

-- =====================================================
-- Index 11: SmenskiIzvestaj - Datum Index
-- Usage: PreradaService for shift reports
-- Priority: MEDIUM
-- Expected Improvement: 20-30% faster date range queries
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_SmenskiIzvestaj_Datum_Status
ON SmenskiIzvestaj(Datum, DokumentStatus, Aktivno);

-- =====================================================
-- Index 12: UgovorProdaja - Status and Date Index
-- Usage: UgovorService for active contracts
-- Priority: HIGH
-- Expected Improvement: 35-45% faster contract queries
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_UgovorProdaja_Status_Datum
ON UgovorProdaja(DokumentStatus, Datum, Aktivno);

-- =====================================================
-- Verification Queries
-- =====================================================

-- Check if indexes were created successfully
SELECT
    TABLE_NAME,
    INDEX_NAME,
    COLUMN_NAME,
    SEQ_IN_INDEX,
    NON_UNIQUE
FROM information_schema.STATISTICS
WHERE TABLE_SCHEMA = 'fruitsysdb_v2'
  AND INDEX_NAME LIKE 'idx_%'
ORDER BY TABLE_NAME, INDEX_NAME, SEQ_IN_INDEX;

-- =====================================================
-- Performance Testing Queries (Before and After)
-- =====================================================

-- Test 1: UgovorService Query (Index 1 & 3)
-- Run before and after creating indexes and compare execution time
EXPLAIN SELECT
    up.ID,
    up.Broj,
    ups.ArtikalID,
    ups.Kolicina,
    COALESCE((
        SELECT SUM(rn.Kolicina)
        FROM RadniNalog rn
        WHERE rn.UgovorProdajaID = up.ID
          AND rn.DokumentStatus = 3
    ), 0) as Isporuceno
FROM UgovorProdaja up
INNER JOIN UgovorProdajaStavka ups ON up.ID = ups.UgovorProdajaID
WHERE up.DokumentStatus = 2 AND up.Aktivno = 1
LIMIT 10;

-- Test 2: Faktura Dashboard Query (Index 7)
EXPLAIN SELECT COUNT(*)
FROM Faktura
WHERE Aktivno = 1
  AND DokumentStatus = 2
  AND Datum >= DATE_SUB(NOW(), INTERVAL 30 DAY);

-- Test 3: EvidencijaRada Query (Index 2)
EXPLAIN SELECT
    er.SmenskiIzvestajID,
    er.RadniNalogID,
    er.BrojRadnika,
    er.BrojRadnihSati
FROM EvidencijaRada er
WHERE er.SmenskiIzvestajID IN (1, 2, 3, 4, 5)
  AND er.DokumentStatus = 2
ORDER BY er.SmenskiIzvestajID;

-- =====================================================
-- Index Statistics
-- =====================================================

-- Monitor index usage after deployment
SELECT
    OBJECT_NAME,
    INDEX_NAME,
    TABLE_ROWS,
    DATA_LENGTH,
    INDEX_LENGTH,
    ROUND(INDEX_LENGTH / DATA_LENGTH, 2) AS INDEX_RATIO
FROM information_schema.TABLES t
INNER JOIN information_schema.STATISTICS s
    ON t.TABLE_NAME = s.TABLE_NAME
    AND t.TABLE_SCHEMA = s.TABLE_SCHEMA
WHERE t.TABLE_SCHEMA = 'fruitsysdb_v2'
  AND s.INDEX_NAME LIKE 'idx_%'
GROUP BY OBJECT_NAME, INDEX_NAME, TABLE_ROWS, DATA_LENGTH, INDEX_LENGTH;

-- =====================================================
-- Maintenance Recommendations
-- =====================================================

-- After creating indexes, analyze tables to update statistics
ANALYZE TABLE RadniNalog;
ANALYZE TABLE EvidencijaRada;
ANALYZE TABLE UgovorProdajaStavka;
ANALYZE TABLE Artikal;
ANALYZE TABLE ArtikalInstanca;
ANALYZE TABLE Faktura;
ANALYZE TABLE OtkupniList;
ANALYZE TABLE Prijemnica;
ANALYZE TABLE Otpremnica;
ANALYZE TABLE SmenskiIzvestaj;
ANALYZE TABLE UgovorProdaja;

-- =====================================================
-- Notes:
-- - Run this script during off-peak hours
-- - Creating indexes on large tables may take time
-- - Monitor server load during index creation
-- - Test queries before/after to measure improvement
-- - Consider running OPTIMIZE TABLE after index creation
-- =====================================================
