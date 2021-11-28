namespace Lyt.World.Model
{
    using Lyt.World.Engine;

    public sealed partial class FluModel : Simulator
    {
        #region Dynamo Model 
        /*
         *     SIMPLE EPIDEMIC MODEL
            NOTE
            L     SUSC.K=SUSC.J+DT*(-INF.JK)
            N     SUSC=988
            NOTE  SUSPECTIBLE POPULATION (PEOPLE)
            R     INF.KL=SICK.K*CNTCTS.K*FRSICK
            NOTE  INFECTION RATE (PEOPLE PER DAY)
            C     FRSICK=0.05
            NOTE  FRACTION OF CONTACTS BECOMING SICK
            NOTE  (DIMENSIONLESS)
            L     SICK.K=SICK.J+DT*(INF.JK-CURE.JK)
            N     SICK=2
            NOTE  SICK POPULATION (PEOPLE)
            A     CNTCTS.K=TABLE(TABCON,SUSC.K/TOTAL,0,1,0.2)
            NOTE  SUSPECTIBLE CONTACTED PER INFECTED PERSON
            NOTE  PER DAY (PEOPLE PER PERSON PER DAY)
            T     TABCON=0/2.8/5.5/8/9.5/10
            NOTE  TABLE FOR CONTACTS
            N     TOTAL=SUSC+SICK+RECOV
            NOTE  TOTAL POPULATION (PEOPLE)
            R     CURE.KL=SICK.K/DUR
            NOTE  CURE RATE (PEOPLE PER DAY)
            C     DUR=10
            NOTE  DURATION OF DISEASE (DAYS)
            L     RECOV.K=RECOV.J+DT*CURE.JK
            N     RECOV=10
            NOTE  RECOVERED POPULATION (PEOPLE)
            NOTE
            SPEC  DT=0.25,LENGTH=50,PRTPER=5,PLTPER=0.5
            PRINT SUSC,SICK,RECOV,INF,CURE
            PLOT  SUSC=W,SICK=S,RECOV=R(0,1000)/INF=I,CURE=C(0,200)
            RUN   SIMPLE
            NOTE
            NOTE  **** MODIFIED MODEL WITH DELAY (INCUBATION)
            NOTE
            EDIT  SIMPLE
            NOTE  INCUBATION DELAY (DAYS)
            C     TSS=3
            NOTE  FRACTION OF CONTACTS SHOWING SYMPTOMS
            NOTE  (DIMENSIONLESS)
            R     SYMP.KL=DELAY1(INF.JK,TSS)
            L     SICK.K=SICK.J+DT*(SYMP.JK-CURE.JK)
            RUN   DELAY
         */
        #endregion Dynamo Model 

        private Auxiliary contacts;
        private Auxiliary population;

        private Auxiliary recoveryRate;
        private Auxiliary deathRate;

        private Level susceptible;
        private Level infected;
        private Level sick;
        private Level recovered;
        private Level dead;

        private Rate infectionRate;
        private Delay sickRate;
        private Delay outcome;
        private Delay vulnerableRate;

        // TODP: Make these parameters 
        private readonly double incubationDays = 3; // days
        private readonly double sicknessDays = 10; // days
        private readonly double rawInfectionRate = 0.05; // dimensionless 
        private readonly double rawLethalityRate = 0.05; // dimensionless 
        private readonly double lostImmunityDays = 6 * 30; // dimensionless 

        private void CreateModel()
        {

            this.sector = "Flu Model";
            this.subSector = "";

            this.population = new Auxiliary("population", "persons")
            {
                UpdateFunction = delegate ()
                {
                    return susceptible.K + infected.K + sick.K + recovered.K;
                }
            };

            this.susceptible = new Level("susceptible", "persons", 1_000_000.0)
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    return susceptible.J - this.DeltaTime * infectionRate.J + vulnerableRate.J;
                }
            };

            this.infected = new Level("infected", "persons", 100.0)
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return infected.J + this.DeltaTime * infectionRate.J;
                }
            };


            this.infectionRate = new Rate("infectionRate", "people per day")
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return
                        this.rawInfectionRate * this.contacts.K * this.infected.K -
                        this.sickRate.K; 
                }
            };

            this.contacts =
                new Table("contacts", "Persons per person and per day",
                new double[] { 0.0, 2.8, 5.5, 8, 9.5, 10 }, 0, 10, 0.5)
                {
                    UpdateFunction = delegate () { return susceptible.K / this.population.K; }
                };


            this.sick = new Level("sick", "persons", 0.0)
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return  sick.J + this.DeltaTime * ( sickRate.J - recoveryRate.J - deathRate.J ) ;
                }

            };

            this.sickRate = new Delay("sickRate", "persons per day", incubationDays, "infected") { };

            this.outcome = new Delay("outcome", "persons", sicknessDays, "sick") { };

            this.recoveryRate = new Auxiliary("recoveryRate", "persons per day")
            {
                UpdateFunction = delegate ()
                {
                    return outcome.K * ( 1.0 - rawLethalityRate ) ;
                }
            };

            this.deathRate = new Auxiliary("deathRate", "persons per day")
            {
                UpdateFunction = delegate ()
                {
                    return outcome.K * rawLethalityRate;
                }
            };

            this.dead = new Level("dead", "persons", 0)
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return dead.J + this.DeltaTime * deathRate.J;
                }
            };

            this.recovered = new Level("recovered", "persons", 0)
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return recovered.J + this.DeltaTime * ( recoveryRate.J - vulnerableRate.J);
                }
            };

            this.vulnerableRate = new Delay("vulnerable", "persons", lostImmunityDays, "recovered") { };
        }

        private readonly string[] dependencies =
        {
            "",
        };
    }
}
