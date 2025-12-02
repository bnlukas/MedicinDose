namespace ordination_test;

using Microsoft.EntityFrameworkCore;

using Service;
using Data;
using shared.Model;

[TestClass]
public class ServiceTest
{
    private DataService service;

    [TestInitialize]
    public void SetupBeforeEachTest()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdinationContext>();
        optionsBuilder.UseInMemoryDatabase(databaseName: "test-database");
        var context = new OrdinationContext(optionsBuilder.Options);
        service = new DataService(context);
        service.SeedData();
    }

    [TestMethod]
    public void PatientsExist()
    {
        Assert.IsNotNull(service.GetPatienter());
    }

    [TestMethod]
    public void OpretDagligFast()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.AreEqual(1, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 2, 1, 0, DateTime.Now, DateTime.Now.AddDays(3));

        Assert.AreEqual(2, service.GetDagligFaste().Count());
    }

    [TestMethod]
[ExpectedException(typeof(ArgumentException))]
public void GetDoser_KasterExceptionVedNegativDosis()
{
    // Arrange
    var ord = new DagligFast(DateTime.Now, DateTime.Now,
        new Laegemiddel("Test", 1, 1, 1, "Stk"),
        -1,  // Morgen dosis negativ → skal give exception
        0,
        0,
        0);

    // Act
    ord.getDoser(); // Denne skal smide exception

    // Hvis exception IKKE bliver kastet, fejler testen automatisk.
}



    [TestMethod]
    public void PN_DoegnDosis_Beregnes_Korrekt()
    {
        // Arrange
        Laegemiddel lm = service.GetLaegemidler().First();
        PN pn = new PN(new DateTime(2025, 12, 2), new DateTime(2025, 12, 4), 6, lm);
        
        // Simuler at der er givet 2 doser på forskellige dage
        pn.dates.Add(new Dato { dato = new DateTime(2025, 12, 2) });
        pn.dates.Add(new Dato { dato = new DateTime(2025, 12, 4) });
        
        // Act
        double doegnDosis = pn.doegnDosis();
        
        // Assert
        // Samlet dosis = 2 * 6 = 12
        // Antal dage mellem første og sidste = 3 dage (2/12 til 4/12)
        // Døgndosis = 12 / 3 = 4
        Assert.AreEqual(4, doegnDosis);
    }

    [TestMethod]
    public void PN_SamletDosis_Beregnes_Korrekt()
    {
        // Arrange
        Laegemiddel lm = service.GetLaegemidler().First();
        PN pn = new PN(new DateTime(2025, 12, 2), new DateTime(2025, 12, 4), 5, lm);
        
        // Simuler at der er givet 3 doser
        pn.dates.Add(new Dato { dato = new DateTime(2025, 12, 2) });
        pn.dates.Add(new Dato { dato = new DateTime(2025, 12, 3) });
        pn.dates.Add(new Dato { dato = new DateTime(2025, 12, 4) });
        
        // Act
        double samletDosis = pn.samletDosis();
        
        // Assert
        Assert.AreEqual(15, samletDosis); // 3 gange * 5 enheder = 15
    }

    // ==================== DagligFast Tests ====================

    [TestMethod]
    public void DagligFast_DoegnDosis_Beregnes_Korrekt()
    {
        // Arrange
        Laegemiddel lm = service.GetLaegemidler().First();
        DagligFast df = new DagligFast(new DateTime(2025, 12, 2), new DateTime(2025, 12, 4), 
                                       lm, 2, 1, 3, 1);
        
        // Act
        double doegnDosis = df.doegnDosis();
        
        // Assert
        Assert.AreEqual(7, doegnDosis); // 2 + 1 + 3 + 1 = 7
    }

    [TestMethod]
    public void DagligFast_SamletDosis_Beregnes_Korrekt()
    {
        // Arrange
        Laegemiddel lm = service.GetLaegemidler().First();
        DagligFast df = new DagligFast(new DateTime(2025, 12, 2), new DateTime(2025, 12, 4), 
                                       lm, 4, 0, 3, 2);
        
        // Act
        double samletDosis = df.samletDosis();
        
        // Assert
        // Døgndosis = 4 + 0 + 3 + 2 = 9
        // Antal dage = 3
        // Samlet dosis = 9 * 3 = 27
        Assert.AreEqual(27, samletDosis);
    }

    // ==================== DagligSkæv Tests ====================

    [TestMethod]
    public void DagligSkaev_DoegnDosis_Skal_Vaere_Implementeret()
    {
        // Arrange
        Laegemiddel lm = service.GetLaegemidler().First();
        Dosis[] doser = new Dosis[]
        {
            new Dosis(new DateTime(2025, 12, 2, 8, 0, 0), 2),
            new Dosis(new DateTime(2025, 12, 2, 14, 0, 0), 3),
            new Dosis(new DateTime(2025, 12, 2, 20, 0, 0), 1)
        };
        DagligSkæv ds = new DagligSkæv(new DateTime(2025, 12, 2), new DateTime(2025, 12, 4), lm, doser);
        
        // Act
        double doegnDosis = ds.doegnDosis();
        
        // Assert - når implementeret, skal dette være summen af alle doser
        // Forventet: 2 + 3 + 1 = 6
        // Men nu returnerer den -1 fordi den ikke er implementeret
        Assert.AreEqual(6, doegnDosis);
    }

    [TestMethod]
    public void DagligSkaev_SamletDosis_Kan_Beregnes_Naar_DoegnDosis_Virker()
    {
        // Arrange
        Laegemiddel lm = service.GetLaegemidler().First();
        Dosis[] doser = new Dosis[]
        {
            new Dosis(new DateTime(2025, 12, 2, 8, 0, 0), 1),
            new Dosis(new DateTime(2025, 12, 2, 20, 0, 0), 2)
        };
        DagligSkæv ds = new DagligSkæv(new DateTime(2025, 12, 2), new DateTime(2025, 12, 4), lm, doser);
        
        // Act
        double samletDosis = ds.samletDosis();
        
        // Assert
        // Antal dage = 3
        // Døgndosis skulle være 1 + 2 = 3
        // Samlet dosis = 3 * 3 = 9
        // Men da doegnDosis returnerer -1, får vi -3
        Assert.AreEqual(9, samletDosis); // 3 dage * (-1) = -3
    }
}