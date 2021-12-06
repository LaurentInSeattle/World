namespace Lyt.World.Model
{
    using Lyt.World.Engine;

    using System;

    public sealed partial class CovidModel : Simulator
    {
        private Auxiliary population;

        private Auxiliary recoveryPerDay;
        private Auxiliary deathPerDay;

        private Level susceptible;  // initial pop'
        private Level infected;     // incubating, non contagious
        private Level contagious;   // incubating, contagious, full mobility
        private Level asymptomatic; // contagious, full mobility
        private Level mild;         // reduced mobility
        private Level sick;         // home isolated 
        private Level serious;      // hospital isolated 
        private Level critical;     // icu isolated 
        private Level recovered;    // immune for a while
        private Level dead;         // the end

        private Rate infectedPerDay;
        private PureDelay sickPerDay;
        private PureDelay outcome;
        private PureDelay vulnerablePerDay;

        // TODO: Make these values simulation parameters 
        private readonly double hospitalBedsRatio = 0.001_5;   // beds per person   (10,244 in WA for 7.6 M pop ) 
        private readonly double icuBedsRatio = 0.000_15;       // beds per person   (1,233 in WA)

        private readonly double contactsFullMobility = 8; // people
        private readonly double contactsReducedMobility = 3; // people

        private readonly double incubationDays = 3; // days
        private readonly double contagiousDays = 7; // days
        private readonly double asymptomaticDays = 10; // days
        private readonly double sicknessDays = 14; // days
        private readonly double seriousDays = 21; // days
        private readonly double criticalDays = 28; // days

        private readonly double asymptomaticRatio = 0.40; // contagious, full mobility
        private readonly double mildRatio = 0.35;         // reduced mobility
        private readonly double sickRatio = 0.15;         // home isolated 
        private readonly double seriousRatio = 0.08;      // hospital isolated 
        private readonly double criticalRatio = 0.02;     // icu isolated 

        private readonly double rawInfectionRate = 0.035; // dimensionless 
        private readonly double rawLethalityRate = 0.025; // dimensionless 

        private readonly double lostImmunityDays =  3 * 30; // dimensionless 

        private void CreateModel()
        {

            this.sector = "Covid Model";
            this.subSector = "";

            this.population = new Auxiliary("population", "persons")
            {
                UpdateFunction = delegate ()
                {
                    return AsInt(susceptible.K + infected.K + sick.K + recovered.K);
                }
            };

            this.susceptible = new Level("susceptible", "persons", 10_000_000.0)
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return AsInt(Positive(susceptible.J - (infectedPerDay.J - vulnerablePerDay.J  ))) ;
                }
            };

            this.infected = new Level("infected", "persons", 0.0)
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    double newInfected = AsInt( infectedPerDay.J - sickPerDay.J) ; 
                    return AsInt(Positive(infected.J + newInfected));
                }
            };


            this.infectedPerDay = new Rate("infectedPerDay", "people per day")
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    double infectedContacts = this.rawInfectionRate * this.contactsFullMobility * this.infected.K; 
                    double value = 0.0; 
                    if ( infectedContacts < this.susceptible.K )
                    {
                        value = infectedContacts;
                    }
                    else
                    {
                        value = this.susceptible.K; 
                    }

                    return AsInt(Positive(value));
                }
            };

            this.sick = new Level("sick", "persons", 0.0)
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    double newSick = AsInt(sickPerDay.J) - recoveryPerDay.J - deathPerDay.J;
                    return AsInt(Positive(sick.J + newSick));
                }

            };

            this.sickPerDay = new PureDelay("sickPerDay", "persons per day", incubationDays, "infectedPerDay") { };

            this.outcome = new PureDelay("outcome", "persons", sicknessDays, "sickPerDay") { };

            this.recoveryPerDay = new Auxiliary("recoveryPerDay", "persons per day")
            {
                UpdateFunction = delegate ()
                {
                    return AsInt(outcome.K * (1.0 - rawLethalityRate));
                }
            };

            this.deathPerDay = new Auxiliary("deathPerDay", "persons per day")
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return AsInt(outcome.K * rawLethalityRate);
                }
            };

            this.dead = new Level("dead", "persons", 0)
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return AsInt(dead.J + deathPerDay.J);
                }
            };

            this.recovered = new Level("recovered", "persons", 0)
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return AsInt(Positive(recovered.J + recoveryPerDay.J - vulnerablePerDay.J ));
                }
            };

            this.vulnerablePerDay = new PureDelay("vulnerablePerDay", "persons", lostImmunityDays, "recoveryPerDay") { };
        }

        private readonly string[] dependencies =
        {
            // Equation         // Depends On
            "outcome",          // sick
            "recoveryPerDay",   // outcome
            "deathPerDay",      // outcome
            "population",       // susceptible infected sick recovered 
            "contacts",         // susceptible population
            "infectedPerDay",   // infected
            "sickPerDay",       // infected per day 
            "vulnerablePerDay", // recovered per day
        };
    }
}
