-- Tabela za praćenje aktivnosti korisnika (login/logout/trajanje sesije)
CREATE TABLE IF NOT EXISTS KorisnikAktivnost (
    ID          BIGINT AUTO_INCREMENT PRIMARY KEY,
    KorisnikIme VARCHAR(100) NOT NULL,
    IpAdresa    VARCHAR(45)  NULL,
    VremeLogina DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    VremeLogauta DATETIME   NULL,
    TrajanjeSekundi INT      NULL,
    INDEX idx_korisnik (KorisnikIme),
    INDEX idx_vreme (VremeLogina)
);
