namespace shared.Model;

public class PN : Ordination {
	public double antalEnheder { get; set; }
    public List<Dato> dates { get; set; } = new List<Dato>();

    public PN (DateTime startDen, DateTime slutDen, double antalEnheder, Laegemiddel laegemiddel) : base(laegemiddel, startDen, slutDen) {
		this.antalEnheder = antalEnheder;
	}

    public PN() : base(null!, new DateTime(), new DateTime()) {
    }

    /// <summary>
    /// Registrerer at der er givet en dosis på dagen givesDen
    /// Returnerer true hvis givesDen er inden for ordinationens gyldighedsperiode og datoen huskes
    /// Returner false ellers og datoen givesDen ignoreres
    /// </summary>
    public bool givDosis(Dato givesDen)
    {
		    DateTime dosisDato = givesDen.dato.Date;

		    if (dosisDato >= startDen && dosisDato < slutDen)
		    {
			    dates.Add(givesDen);
			    return true;
		    }

		    return false;
    }

    public override double doegnDosis() {
	    double sum = 0; 
	    if (dates.Count > 0)
	    {
		    DateTime min = dates.First().dato.Date;
		    DateTime max = dates.First().dato.Date;

		    foreach (Dato d in dates) {
			    if (d.dato.Date < min) {
				    min = d.dato.Date; }
			    if(d.dato.Date > max ) {
				    max = d.dato.Date;}
			    
		    }
		    int dage = (max - min).Days +1;
		    sum = samletDosis() / dage; }

	    return sum; 
    }


    public override double samletDosis() {
        return dates.Count() * antalEnheder;
    }

    public int getAntalGangeGivet() {
        return dates.Count();
    }

	public override String getType() {
		return "PN";
	}
}
