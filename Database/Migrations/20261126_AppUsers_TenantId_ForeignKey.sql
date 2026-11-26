-- Ontbrekende foreign key op AppUsers.TenantId (kolom toegevoegd in 20260810_AppUsers_TenantId.sql
-- zonder FK). Geen wijzende rijen gevonden bij controle op productie (0 orphans), dus direct toe
-- te voegen. NULL blijft toegestaan (Beheerder-gebruikers hebben geen tenant).
ALTER TABLE dbo.AppUsers
    ADD CONSTRAINT FK_AppUsers_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id);
GO
