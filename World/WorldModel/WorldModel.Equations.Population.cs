namespace Lyt.World.Model
{
    using Lyt.World.Engine;

    using System;

    // Model Variables: Creates the Population Sector - Equations #1 to #48
    public sealed partial class WorldModel
    {
        const double subsistenceFoodPerCapitaK = 230.0;  // kilograms per person-year, used in eqns 20, 127

        private Level population0To14;
        private Level population15To44;
        private Level population45To64;
        private Level population65AndOver;


        private Rate deathsPerYear0To14;
        private Rate deathsPerYear15To44;
        private Rate deathsPerYear45To64;
        private Rate deathsPerYear65AndOver;
        private Rate maturationsPerYear14to15;
        private Rate maturationsPerYear44to45;
        private Rate maturationsPerYear64to65;
        private Rate birthsPerYear; 

        private Auxiliary population;
        private Auxiliary deathsPerYear;
        private Auxiliary crudeDeathRate;
        private Auxiliary lifeExpectancy;
        private Auxiliary lifetimeMultiplierFromHealthServices;
        private Auxiliary lifetimeMultiplierFromCrowding;
        private Auxiliary fertilityControlAllocationPerCapita;
        private Auxiliary familyIncomeExpectation;
        private Auxiliary desiredCompletedFamilySize;
        private Auxiliary desiredTotalFertility;
        private Auxiliary maxTotalFertility;
        private Auxiliary totalFertility;
        private Auxiliary crudeBirthRate; 

        private Table mortality0To14;
        private Table mortality15To44;
        private Table mortality45To64;
        private Table mortality65AndOver;
        private Table lifetimeMultiplierFromFood;
        private Table healthServicesAllocationsPerCapita;
        private Table lifetimeMultiplierFromHealthServicesBefore;
        private Table lifetimeMultiplierFromHealthServicesAfter;
        private Table fractionOfPopulationUrban;
        private Table crowdingMultiplierFromIndustrialization;
        private Table lifetimeMultiplierFromPollution;
        private Table fractionOfServicesAllocatedToFertilityControl;
        private Table fertilityControlEffectiveness;
        private Table familyResponseToSocialNorm;
        private Table socialFamilySizeNorm;
        private Table compensatoryMultiplierFromPerceivedLifeExpectancy;
        private Table fecundityMultiplier;

        private Smooth effectiveHealthServicesPerCapita;
        private Smooth averageIndustrialOutputPerCapita; 

        private Delay fertilityControlFacilitiesPerCapita;
        private Delay delayedIndustrialOutputPerCapita;
        private Delay perceivedLifeExpectancy; 

        private void CreatePopulationSector()
        {
            this.sector = "Population";

            #region The Population Subsector

            this.subSector = "Population";

            this.population = new Auxiliary(this, "population", 1, "persons")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                { 
                    return population0To14.K + population15To44.K + population45To64.K + population65AndOver.K; 
                }
            };

            this.population0To14 = new Level(this, "population0To14", 2, "persons", 6.5e8)
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    return population0To14.J + this.DeltaTime * (birthsPerYear.J - deathsPerYear0To14.J - maturationsPerYear14to15.J);

                }
            };

            this.deathsPerYear0To14 = new Rate(this, "deathsPerYear0To14", 3, "persons per year")
            {
                UpdateFunction = delegate () { return population0To14.K * mortality0To14.K; }
            };

            this.mortality0To14 = new Table(
                this, "mortality0To14", 4, "deaths per person-year",
                new double[] { 0.0567, 0.0366, 0.0243, 0.0155, 0.0082, 0.0023, 0.0010 }, 20, 80, 10)
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return lifeExpectancy.K; },
            };

            this.maturationsPerYear14to15 = new Rate(this, "maturationsPerYear14to15", 5, "persons per year")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return population0To14.K * (1 - mortality0To14.K) / 15.0; },
            };

            this.population15To44 = new Level(this, "population15To44", 6, "persons", 7.0e8)
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    return
                        population15To44.J + 
                        this.DeltaTime * (maturationsPerYear14to15.J - deathsPerYear15To44.J - maturationsPerYear44to45.J);
                },
            };

            this.deathsPerYear15To44 = new Rate(this, "deathsPerYear15To44", 7, "persons per year")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return population15To44.K * mortality15To44.K; },
            };

            this.mortality15To44 =
                new Table(this, "mortality15To44", 8, "deaths per person-year",
                            new double[] { 0.0266, 0.0171, 0.0110, 0.0065, 0.0040, 0.0016, 0.0008 }, 20, 80, 10)
                {
                    CannotBeNegative = true,
                    CannotBeZero = true,
                    UpdateFunction = delegate () { return lifeExpectancy.K; },
                };

            this.maturationsPerYear44to45 = new Rate(this, "maturationsPerYear44to45", 9, "persons per year")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return population15To44.K * (1 - mortality15To44.K) / 30.0; },
            };

            this.population45To64 = new Level(this, "population45To64", 10, "persons", 1.9e8)
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    return
                        population45To64.J +
                        this.DeltaTime * (maturationsPerYear44to45.J - deathsPerYear45To64.J - maturationsPerYear64to65.J);
                },
            };

            this.deathsPerYear45To64 = new Rate(this, "deathsPerYear45To64", 11, "persons per year")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return population45To64.K * mortality45To64.K; },
            };

            this.mortality45To64 =
                new Table(
                    this, "mortality45To64", 12, "deaths per person-year",
                    new double[] { 0.0562, 0.0373, 0.0252, 0.0171, 0.0118, 0.0083, 0.0060 }, 20, 80, 10)
                {
                    CannotBeNegative = true,
                    CannotBeZero = true,
                    UpdateFunction = delegate () { return lifeExpectancy.K; },
                };

            this.maturationsPerYear64to65 = new Rate(this, "maturationsPerYear64to65", 13, "persons per year")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return population45To64.K * (1 - mortality45To64.K) / 20.0; },
            };


            this.population65AndOver = new Level(this, "population65AndOver", 14, "persons", 6.0e7)
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    return population65AndOver.J + this.DeltaTime * (maturationsPerYear64to65.J - deathsPerYear65AndOver.J);
                },
            };

            this.deathsPerYear65AndOver = new Rate(this, "deathsPerYear65AndOver", 15, "persons per year")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return population65AndOver.K * mortality65AndOver.K; },
            };

            this.mortality65AndOver =
                new Table(
                    this, "mortality65AndOver", 16, "deaths per person-year",
                    new double[] { 0.13, 0.11, 0.09, 0.07, 0.06, 0.05, 0.04 }, 20, 80, 10)
                {
                    CannotBeNegative = true,
                    CannotBeZero = true,
                    UpdateFunction = delegate () { return lifeExpectancy.K; },
                };


            #endregion The Population Subsector

            #region The Death-Rate Subsector

            this.subSector = "Death Rate";

            this.deathsPerYear = new Auxiliary(this, "deathsPerYear", 17, "persons per year")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    return deathsPerYear0To14.J + deathsPerYear15To44.J + deathsPerYear45To64.J + deathsPerYear65AndOver.J;
                },
            };

            this.crudeDeathRate = new Auxiliary(this, "crudeDeathRate", 18, "deaths per 1000 person-years")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return 1000 * deathsPerYear.K / population.K; }
            };

            this.lifeExpectancy = new Auxiliary(this, "lifeExpectancy", 19, "years")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    const double lifeExpectancyNormal = 32.0;
                    return 
                        lifeExpectancyNormal * lifetimeMultiplierFromFood.K * lifetimeMultiplierFromHealthServices.K *
                        lifetimeMultiplierFromPollution.K * lifetimeMultiplierFromCrowding.K;
                },
            };

            this.lifetimeMultiplierFromFood =
                new Table(
                    this, "lifetimeMultiplierFromFood", 20, "dimensionless",
                    new double[] { 0, 1, 1.2, 1.3, 1.35, 1.4 }, 0, 5, 1)
            {
                    CannotBeNegative = true,
                    UpdateFunction = delegate () { return foodPerCapita.K / subsistenceFoodPerCapitaK; }
            };


            this.healthServicesAllocationsPerCapita =
                new Table(
                    this, "healthServicesAllocationsPerCapita", 21, "dollars per person-year",
                    new double[] { 0, 20, 50, 95, 140, 175, 200, 220, 230 }, 0, 2000, 250)
                {
                    CannotBeNegative = true,
                    UpdateFunction = delegate () { return serviceOutputPerCapita.K ; }
                };


            const double effectiveHealthServicesPerCapitaImpactDelay = 20.0; // years, used in eqn 22
            this.effectiveHealthServicesPerCapita = 
                new Smooth(
                    this, "effectiveHealthServicesPerCapita", 22, "dollars per person-year", 
                    effectiveHealthServicesPerCapitaImpactDelay, "healthServicesAllocationsPerCapita", 0.001);

            this.lifetimeMultiplierFromHealthServices =
                new Auxiliary(this, "lifetimeMultiplierFromHealthServices", 23)
                {
                    UpdateFunction = delegate ()
                    {
                        const double lifetimeMultiplierFromHealthServicesPolicyYear = 1940.0;
                        return Clip(
                            lifetimeMultiplierFromHealthServicesAfter.K, lifetimeMultiplierFromHealthServicesBefore.K,
                            this.Time, lifetimeMultiplierFromHealthServicesPolicyYear);
                    }
                };


            this.lifetimeMultiplierFromHealthServicesBefore =
                new Table(
                    this, "lifetimeMultiplierFromHealthServicesBefore", 24, "dimensionless" ,
                    new double[] { 1, 1.1, 1.4, 1.6, 1.7, 1.8 } , 0, 100, 20)
                {
                    UpdateFunction = delegate () { return effectiveHealthServicesPerCapita.K; }
                };


            this.lifetimeMultiplierFromHealthServicesAfter =
                new Table(
                    this, "lifetimeMultiplierFromHealthServicesAfter", 24, "dimensionless",
                    new double[] { 1, 1.4, 1.6, 1.8, 1.95, 2.0 }, 0, 100, 20)
                {
                    UpdateFunction = delegate () { return effectiveHealthServicesPerCapita.K; }
                };

            this.fractionOfPopulationUrban = 
                new Table(
                    this, "fractionOfPopulationUrban", 26, "dimensionless",
                    // 16 billion 
                    new double[] { 0, 0.2, 0.4, 0.5, 0.58, 0.65, 0.72, 0.78, 0.80 }, 0, 1.6e10, 2.0e9)
                {
                    UpdateFunction = delegate () { return population.K; }
                };

            this.crowdingMultiplierFromIndustrialization =
                new Table(
                    this, "crowdingMultiplierFromIndustrialization", 27, "dimensionless",
                    new double[] { 0.5, 0.05, -0.1, -0.08, -0.02, 0.05, 0.1, 0.15, 0.2 }, 0, 1600, 200)
                {
                    UpdateFunction = delegate () { return industrialOutputPerCapita.K; }
                };

            this.lifetimeMultiplierFromCrowding = new Auxiliary(this, "lifetimeMultiplierFromCrowding", 28)
            {
                UpdateFunction = delegate () 
                { 
                    return 1.0 - (crowdingMultiplierFromIndustrialization.K * fractionOfPopulationUrban.K); 
                }
            };

            this.lifetimeMultiplierFromPollution = 
                new Table(
                    this, "lifetimeMultiplierFromPollution", 29, "dimensionless",
                    new double[] { 1.0, 0.99, 0.97, 0.95, 0.90, 0.85, 0.75, 0.65, 0.55, 0.40, 0.20 }, 0, 100, 10)
                {
                    UpdateFunction = delegate () { return indexOfPersistentPollution.K; }
                };

            #endregion The Death-Rate Subsector

            #region The Birth-Rate Subsector

            this.subSector = "Birth Rate";

            this.birthsPerYear = new Rate(this, "birthsPerYear", 30, "persons per year")
            {
                UpdateFunction = delegate () 
                {
                    const double birthsPerYearReproductiveLifetime = 30.0 ;        // years
                    const double birthsPerYearPopulationEquilibriumTime = 4000.0;  // years
                    var after = deathsPerYear.K;
                    var before = totalFertility.K * population15To44.K * 0.5 / birthsPerYearReproductiveLifetime;
                    return WorldModel.Clip(after, before, this.Time, birthsPerYearPopulationEquilibriumTime);
                },
            };

            this.crudeBirthRate = new Auxiliary(this, "crudeBirthRate", 31, "births per 1000 person-years")
            {
                UpdateFunction = delegate () { return 1000.0 * birthsPerYear.J / population.K; }
            };

            this.totalFertility = new Auxiliary(this, "totalFertility", 32)
            {
                UpdateFunction = delegate ()
                {
                    return Math.Min(
                        maxTotalFertility.K,
                        maxTotalFertility.K * (1 - fertilityControlEffectiveness.K) + desiredTotalFertility.K * fertilityControlEffectiveness.K);
                }
            };

            const double maxTotalFertilityNormal = 12.0;   // dimensionless
            this.maxTotalFertility = new Auxiliary(this, "maxTotalFertility", 33)
            {
                UpdateFunction = delegate () { return maxTotalFertilityNormal * fecundityMultiplier.K; }
            };

            this.fecundityMultiplier = 
                new Table(this, "fecundityMultiplier", 34, "dimensionless",
                new double[] { 0.0, 0.2, 0.4, 0.6, 0.8, 0.9, 1.0, 1.05, 1.1 }, 0, 80, 10)
                {
                    UpdateFunction = delegate () { return lifeExpectancy.K; }
                };

            this.desiredTotalFertility = new Auxiliary(this,"desiredTotalFertility", 35)
            {
                UpdateFunction = delegate () { return desiredCompletedFamilySize.K * compensatoryMultiplierFromPerceivedLifeExpectancy.K; }
            };

            this.compensatoryMultiplierFromPerceivedLifeExpectancy =
                new Table(
                    this, "compensatoryMultiplierFromPerceivedLifeExpectancy", 36, "dimensionless",
                    new double[] { 3.0, 2.1, 1.6, 1.4, 1.3, 1.2, 1.1, 1.05, 1.0 }, 0, 80, 10)
                {
                    UpdateFunction = delegate () { return perceivedLifeExpectancy.K; }
                };

            const double lifetimePerceptionDelayK = 20.0;      // years, used in eqn 37
            this.perceivedLifeExpectancy = 
                new Delay(this, "perceivedLifeExpectancy", 37, "years", lifetimePerceptionDelayK, "lifeExpectancy");

            const double desiredCompletedFamilySizeNormal = 4.0;
            const double zeroPopulationGrowthTargetYear = 4000;
            this.desiredCompletedFamilySize = new Auxiliary(this, "desiredCompletedFamilySize", 38, "dimensionless") // not persons?
            {
                UpdateFunction = delegate ()
                {
                    return Clip( 
                            2.0, desiredCompletedFamilySizeNormal * familyResponseToSocialNorm.K * socialFamilySizeNorm.K, 
                            this.Time, zeroPopulationGrowthTargetYear);
                }
            };

            this.socialFamilySizeNorm = new Table(
                this, "socialFamilySizeNorm", 39, "dimensionless",
                new double[] { 1.25, 1, 0.9, 0.8, 0.75 }, 0, 800, 200)
            {
                UpdateFunction = delegate () { return delayedIndustrialOutputPerCapita.K; }
            };

            const double socialAdjustmentDelayK = 20.0;    // years, used in eqn 40
            this.delayedIndustrialOutputPerCapita = 
                new Delay(
                    this, "delayedIndustrialOutputPerCapita", 40, "dollars per person-year", 
                    socialAdjustmentDelayK, "industrialOutputPerCapita");

            this.familyResponseToSocialNorm = 
                new Table(
                    this, "familyResponseToSocialNorm", 41, "dimensionless",
                    new double[] { 0.5, 0.6, 0.7, 0.85, 1.0 } , -0.2, 0.2, 0.1)
            {
                UpdateFunction = delegate () { return familyIncomeExpectation.K; }
            };

            this.familyIncomeExpectation = new Auxiliary(this, "familyIncomeExpectation", 42, "dimensionless")
            {
                UpdateFunction = delegate ()
                {
                    return (industrialOutputPerCapita.K - averageIndustrialOutputPerCapita.K) / averageIndustrialOutputPerCapita.K;
                }
            };

            const double incomeExpectationAveragingTimeK = 3.0; // years, used in eqn 43
            this.averageIndustrialOutputPerCapita =
                new Smooth(
                    this, "averageIndustrialOutputPerCapita", 43, "dollars per person-year",
                    incomeExpectationAveragingTimeK, "industrialOutputPerCapita", 0.001)
                {
                    CannotBeZero = true
                };

            var needForFertilityControl = new Auxiliary(this, "needForFertilityControl", 44)
            {
                UpdateFunction = delegate () { return (maxTotalFertility.K / desiredTotalFertility.K) - 1; }
            };

            this.fertilityControlEffectiveness =
                new Table(
                    this, "fertilityControlEffectiveness", 45, "dimensionless",
                    new double[] { 0.75, 0.85, 0.90, 0.95, 0.98, 0.99, 1.0 }, 0, 3, 0.5)
                {
                    UpdateFunction = delegate () { return fertilityControlFacilitiesPerCapita.K; }
                };

            const double healthServicesImpactDelayK = 20.0;    // years, for eqn 46
            this.fertilityControlFacilitiesPerCapita = 
                new Delay(
                    this, "fertilityControlFacilitiesPerCapita", 46, "dollars per person-year", 
                    healthServicesImpactDelayK, "fertilityControlAllocationPerCapita");

            this.fertilityControlAllocationPerCapita =
                new Auxiliary(this, "fertilityControlAllocationPerCapita", 47, "dollars per person-year")
                {
                    UpdateFunction = delegate ()
                    {
                        return fractionOfServicesAllocatedToFertilityControl.K * serviceOutputPerCapita.K;
                    }
                };

            this.fractionOfServicesAllocatedToFertilityControl =
                new Table(
                    this, "fractionOfServicesAllocatedToFertilityControl", 48, "dimensionless",
                    new double[] { 0.0, 0.005, 0.015, 0.025, 0.030, 0.035 }, 0, 10, 2)
                {
                    UpdateFunction = delegate () { return needForFertilityControl.K; }
                };

            #endregion The Birth-Rate Subsector
        }
    }
}
