-- Cross-tenant IBAN-collisiecheck: voorkomt dat een GoDutch-bankrekening die al bij
-- tenant X geregistreerd staat, onopgemerkt ook bij tenant Y getoond/aangemaakt wordt
-- (zie GoDutch-melding "Bankrekeningen van andere klant zichtbaar via GoDutch API-subaccount").
CREATE OR ALTER PROCEDURE [dbo].[BankAccounts_GetByIban]
    @Iban nvarchar(34)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [Id],
        [TenantId],
        [Iban],
        [AccountName],
        [IsActive],
        [SnelStartGrootboekId],
        [SnelStartGrootboekNummer],
        [SnelStartGrootboekNaam],
        [SnelStartDagboekId],
        [SnelStartDagboekCode],
        [SnelStartDagboekNaam],
        [CreatedUtc],
        [ModifiedUtc]
    FROM [dbo].[BankAccounts]
    WHERE REPLACE(UPPER([Iban]), ' ', '') = REPLACE(UPPER(@Iban), ' ', '');
END
