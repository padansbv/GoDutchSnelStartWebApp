-- Leegmaken van de database voor een frisse start: verwijdert ALLE tenants, gebruikers,
-- koppelingen, bankrekeningen, importruns, mappings en exportbatches.
--
-- NIET geraakt (bewust):
--   - dbo.MyPosTransactionTypeDefinitions  (vaste referentiedata, geen tenant-data)
--   - dbo.MyPosAutoSyncSettings            (één globale instellingenrij voor de achtergrondworker,
--                                            geen tenant-data)
--
-- Verwijdert WEL dbo.GoDutchLeads (aanmeldingen via de GoDutch-landingspagina). Wil je die
-- behouden (bijv. voor sales-opvolging), comment dan dat blok hieronder uit.
--
-- Volgorde is van kind naar ouder (foreign keys). Draait in één transactie: bij een fout
-- wordt alles teruggedraaid en blijft de database ongewijzigd.

BEGIN TRANSACTION;

BEGIN TRY

    -- Bankrekening-gerelateerd (kind van BankAccounts)
    DELETE FROM dbo.BankAccountSnelStartLinks;
    DELETE FROM dbo.BankAccountSettings;
    DELETE FROM dbo.GoDutchImportRuns;

    -- MyPos exportbatches en losse transacties (onderlinge afhankelijkheid: regels en
    -- ruwe transacties verwijzen naar de batch, dus eerst die, dan de batch zelf)
    DELETE FROM dbo.MyPosExportBatchLines;
    DELETE FROM dbo.MyPosRawTransactions;
    DELETE FROM dbo.MyPosExportBatches;

    -- MyPos mappings (verwijst naar MyPosTransactionTypeDefinitions, maar die blijft staan)
    DELETE FROM dbo.MyPosTransactionTypeMappings;

    -- Overig tenant-gebonden
    DELETE FROM dbo.Notifications;
    DELETE FROM dbo.PasswordResetTokens;
    DELETE FROM dbo.AppUsers;
    DELETE FROM dbo.BankAccounts;
    DELETE FROM dbo.SnelStartAdministrations;
    DELETE FROM dbo.TenantMyPosConnections;
    DELETE FROM dbo.TenantGoDutchConnections;
    DELETE FROM dbo.TenantSnelStartConnections;

    -- GoDutch-aanmeldingen via de landingspagina (zie opmerking hierboven)
    DELETE FROM dbo.GoDutchLeads;

    -- Tenants zelf, als laatste
    DELETE FROM dbo.Tenants;

    COMMIT TRANSACTION;
    PRINT 'Database leeggemaakt: alle tenants, gebruikers en gekoppelde data verwijderd.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'FOUT — rollback uitgevoerd:';
    PRINT ERROR_MESSAGE();
    THROW;
END CATCH;
