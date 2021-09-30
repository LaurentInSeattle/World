namespace Lyt.World.Model
{
    // Model Variables: Creates the Capital Sector - Equations #49 to #83
    public sealed partial class Model
    {
        private Level industrialCapital;
        private Level serviceCapital;

        private Rate industrialCapitalDepreciationRate;
        private Rate industrialCapitalInvestmentRate;
        private Rate serviceCapitalInvestmentRate;
        private Rate serviceCapitalDepreciationRate;


        private Auxiliary industrialOutputPerCapita;
        private Auxiliary industrialOutput;
        private Auxiliary industrialCapitalOutputRatio;
        private Auxiliary fractionOfIndustrialOutputAllocatedToIndustry;
        private Auxiliary fractionOfIndustrialOutputAllocatedToConsumption;
        private Auxiliary fractionOfIndustrialOutputAllocatedToConsumptionConstant;
        private Auxiliary averageLifetimeOfIndustrialCapital;
        private Auxiliary indicatedServiceOutputPerCapita;
        private Auxiliary fractionOfIndustrialOutputAllocatedToServices;
        private Auxiliary averageLifetimeOfServiceCapital;
        private Auxiliary serviceOutput;
        private Auxiliary serviceOutputPerCapita;
        private Auxiliary serviceCapitalOutputRatio;
        private Auxiliary jobs;
        private Auxiliary potentialJobsInIndustrialSector;
        private Auxiliary potentialJobsInServiceSector;
        private Auxiliary potentialJobsInAgriculturalSector;
        private Auxiliary laborForce;
        private Auxiliary laborUtilizationFraction;

        private Table fractionOfIndustrialOutputAllocatedToConsumptionVariable;
        private Table indicatedServiceOutputPerCapitaAfter;
        private Table indicatedServiceOutputPerCapitaBefore;
        private Table fractionOfIndustrialOutputAllocatedToServicesAfter;
        private Table fractionOfIndustrialOutputAllocatedToServicesBefore;
        private Table jobsPerIndustrialCapitalUnit;
        private Table jobsPerServiceCapitalUnit;
        private Table jobsPerHectare;
        private Table capitalUtilizationFraction;

        private Smooth laborUtilizationFractionDelayed;

        // Always 0.43 ??? Possibly a bug ? 
        private double fractionOfIndustrialOutputAllocatedToConsumptionConstantBefore = 0.43;
        private double fractionOfIndustrialOutputAllocatedToConsumptionConstantAfter = 0.43;

        public void SetOutputConsumed(double fraction)
        {
            this.fractionOfIndustrialOutputAllocatedToConsumptionConstantBefore = fraction;
            this.fractionOfIndustrialOutputAllocatedToConsumptionConstantAfter = fraction;
        }

        private void CreateCapitalSector()
        {
            this.sector = "Capital";

            // The Industrial Subsector
            #region The Industrial Subsector

            this.subSector = "Industrial";

            this.industrialOutputPerCapita = new Auxiliary(this, "industrialOutputPerCapita", 49, "dollars per person-year")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return this.industrialOutput.K / population.K; }
            };

            this.industrialOutput = new Auxiliary(this, "industrialOutput", 50, "dollars per year")
            {
                UpdateFunction = delegate ()
                {
                    return industrialCapital.K *
                        (1 - fractionOfCapitalAllocatedToObtainingResources.K) * capitalUtilizationFraction.K / industrialCapitalOutputRatio.K;
                }
            };

            // Always 3.0 ??? Possibly a bug ? 
            const double industrialCapitalOutputRatioBefore = 3.0;
            const double industrialCapitalOutputRatioAfter = 3.0;
            this.industrialCapitalOutputRatio = new Auxiliary(this, "industrialCapitalOutputRatio", 51, "years")
            {
                UpdateFunction = delegate ()
                {
                    return Clip(industrialCapitalOutputRatioAfter, industrialCapitalOutputRatioBefore, this.Time, Model.PolicyYear);
                }
            };

            this.industrialCapital = new Level(this, "industrialCapital", 52, "dollars", 2.1e11)
            {
                UpdateFunction = delegate ()
                {
                    return
                        industrialCapital.J +
                        this.DeltaTime * (industrialCapitalInvestmentRate.J - industrialCapitalDepreciationRate.J);
                }
            };

            this.industrialCapitalDepreciationRate = new Rate(this, "industrialCapitalDepreciationRate", 53, "dollars per year")
            {
                UpdateFunction = delegate () { return industrialCapital.K / averageLifetimeOfIndustrialCapital.K; }
            };

            // Always 14.0 ??? Possibly a bug ? 
            const double averageLifetimeOfIndustrialCapitalBefore = 14.0;
            const double averageLifetimeOfIndustrialCapitalAfter = 14.0;
            this.averageLifetimeOfIndustrialCapital = new Auxiliary(this, "averageLifetimeOfIndustrialCapital", 54, "years")
            {
                UpdateFunction = delegate ()
                {
                    return
                        Clip(averageLifetimeOfIndustrialCapitalAfter, averageLifetimeOfIndustrialCapitalBefore,
                        this.Time, Model.PolicyYear);
                }
            };

            this.industrialCapitalInvestmentRate = new Rate(this, "industrialCapitalInvestmentRate", 55, "dollars per year")
            {
                UpdateFunction = delegate () { return industrialOutput.K * fractionOfIndustrialOutputAllocatedToIndustry.K; }
            };

            this.fractionOfIndustrialOutputAllocatedToIndustry = new Auxiliary(this, "fractionOfIndustrialOutputAllocatedToIndustry", 56)
            {
                UpdateFunction = delegate ()
                {
                    return
                        1 -
                        fractionOfIndustrialOutputAllocatedToAgriculture.K -
                        fractionOfIndustrialOutputAllocatedToServices.K -
                        fractionOfIndustrialOutputAllocatedToConsumption.K;
                }
            };

            const double fractionOfIndustrialOutputAllocatedToConsumptionIndustrialEquilibriumTime = 4000.0;  // year
            this.fractionOfIndustrialOutputAllocatedToConsumption =
                new Auxiliary(this, "fractionOfIndustrialOutputAllocatedToConsumption", 57)
                {
                    UpdateFunction = delegate ()
                    {
                        return
                            Clip(
                                fractionOfIndustrialOutputAllocatedToConsumptionVariable.K,
                                fractionOfIndustrialOutputAllocatedToConsumptionConstant.K,
                                this.Time, fractionOfIndustrialOutputAllocatedToConsumptionIndustrialEquilibriumTime);
                    }
                };


            // Always 0.43 ??? Possibly a bug ? 
            const double fractionOfIndustrialOutputAllocatedToConsumptionConstantBefore = 0.43;
            const double fractionOfIndustrialOutputAllocatedToConsumptionConstantAfter = 0.43;
            this.fractionOfIndustrialOutputAllocatedToConsumptionConstant =
                new Auxiliary(this, "fractionOfIndustrialOutputAllocatedToConsumptionConstant", 58)
                {
                    UpdateFunction = delegate ()
                    {
                        return
                            Clip(
                                fractionOfIndustrialOutputAllocatedToConsumptionConstantAfter,
                                fractionOfIndustrialOutputAllocatedToConsumptionConstantBefore,
                                this.Time, Model.PolicyYear);
                    }
                };

            const double fractionOfIndustrialOutputAllocatedToConsumptionVariableIndustrialOutputPerCapitaDesired = 400.0;
            this.fractionOfIndustrialOutputAllocatedToConsumptionVariable =
                new Table(this, "fractionOfIndustrialOutputAllocatedToConsumptionVariable", 59, "dimensionless",
                new double[] { 0.3, 0.32, 0.34, 0.36, 0.38, 0.43, 0.73, 0.77, 0.81, 0.82, 0.83 }, 0, 2, 0.2)
                {
                    UpdateFunction = delegate ()
                    {
                        return
                            industrialOutputPerCapita.K /
                            fractionOfIndustrialOutputAllocatedToConsumptionVariableIndustrialOutputPerCapitaDesired;
                    }
                };

            #endregion The Industrial Subsector


            // The Service Subsector
            #region The Service Subsector

            this.subSector = "Services";

            this.indicatedServiceOutputPerCapita = new Auxiliary(this, "indicatedServiceOutputPerCapita", 60, "dollars per person-year")
            {
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    // Bug ??? Both 'After' and 'Before' tables below are identical 
                    return
                        Clip(
                            indicatedServiceOutputPerCapitaAfter.K, indicatedServiceOutputPerCapitaBefore.K,
                            this.Time, Model.PolicyYear);
                }
            };


            this.indicatedServiceOutputPerCapitaBefore =
                new Table(this, "indicatedServiceOutputPerCapitaBefore", 61, "dollars per person-year",
                    new double[] { 40, 300, 640, 1000, 1220, 1450, 1650, 1800, 2000 }, 0, 1600, 200)
                {
                    UpdateFunction = delegate () { return industrialOutputPerCapita.K; }
                };

            this.indicatedServiceOutputPerCapitaAfter =
                new Table(this, "indicatedServiceOutputPerCapitaAfter", 62, "dollars per person-year",
                    new double[] { 40, 300, 640, 1000, 1220, 1450, 1650, 1800, 2000 }, 0, 1600, 200)
                {
                    UpdateFunction = delegate () { return industrialOutputPerCapita.K; }
                };


            this.fractionOfIndustrialOutputAllocatedToServices =
                new Auxiliary(this, "fractionOfIndustrialOutputAllocatedToServices", 63)
                {
                    CannotBeZero = true,
                    UpdateFunction = delegate ()
                    {
                        // Bug ??? Both 'After' and 'Before' tables below are identical 
                        return
                            Clip(
                                fractionOfIndustrialOutputAllocatedToServicesAfter.K,
                                fractionOfIndustrialOutputAllocatedToServicesBefore.K,
                                this.Time, Model.PolicyYear);
                    }
                };

            this.fractionOfIndustrialOutputAllocatedToServicesBefore =
                new Table(this, "fractionOfIndustrialOutputAllocatedToServicesBefore", 64, "dimensionless",
                    new double[] { 0.3, 0.2, 0.1, 0.05, 0 }, 0, 2, 0.5)
                {
                    UpdateFunction = delegate () { return serviceOutputPerCapita.K / indicatedServiceOutputPerCapita.K; }
                };

            this.fractionOfIndustrialOutputAllocatedToServicesAfter =
                new Table(this, "fractionOfIndustrialOutputAllocatedToServicesAfter", 65, "dimensionless",
                    new double[] { 0.3, 0.2, 0.1, 0.05, 0 }, 0, 2, 0.5)
                {
                    UpdateFunction = delegate () { return serviceOutputPerCapita.K / indicatedServiceOutputPerCapita.K; }
                };

            this.serviceCapitalInvestmentRate = new Rate(this, "serviceCapitalInvestmentRate", 66, "dollars per year")
            {
                UpdateFunction = delegate () { return industrialOutput.K * fractionOfIndustrialOutputAllocatedToServices.K; }
            };

            this.serviceCapital = new Level(this, "serviceCapital", 67, "dollars", 1.44e11)
            {
                UpdateFunction = delegate ()
                {
                    return serviceCapital.J + this.DeltaTime * (serviceCapitalInvestmentRate.J - serviceCapitalDepreciationRate.J);
                }
            };

            this.serviceCapitalDepreciationRate = new Rate(this, "serviceCapitalDepreciationRate", 68, "dollars per year")
            {
                UpdateFunction = delegate () { return serviceCapital.K / averageLifetimeOfServiceCapital.K; }
            };


            // Bug ??? Both 'After' and 'Before' values above are identical 
            const double averageLifetimeOfServiceCapitalBefore = 20.0;   // years
            const double averageLifetimeOfServiceCapitalAfter = 20.0;    // years
            this.averageLifetimeOfServiceCapital = new Auxiliary(this, "averageLifetimeOfServiceCapital", 69, "years")
            {
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    return
                        Clip(
                            averageLifetimeOfServiceCapitalAfter, averageLifetimeOfServiceCapitalBefore,
                            this.Time, Model.PolicyYear);
                }
            };

            this.serviceOutput = new Auxiliary(this, "serviceOutput", 70, "dollars per year")
            {
                UpdateFunction = delegate ()
                {
                    return (serviceCapital.K * capitalUtilizationFraction.K) / serviceCapitalOutputRatio.K;
                }
            };

            this.serviceOutputPerCapita = new Auxiliary(this, "serviceOutputPerCapita", 71, "dollars per person-year")
            {
                UpdateFunction = delegate () { return serviceOutput.K / population.K; }
            };

            // Bug ??? Both 'After' and 'Before' values are identical 
            const double serviceCapitalOutputRatioBefore = 1.0;
            const double serviceCapitalOutputRatioAfter = 1.0;
            this.serviceCapitalOutputRatio = new Auxiliary(this, "serviceCapitalOutputRatio", 72, "years")
            {
                UpdateFunction = delegate ()
                {
                    return Clip(serviceCapitalOutputRatioAfter, serviceCapitalOutputRatioBefore, this.Time, Model.PolicyYear);
                }
            };

            #endregion The Service Subsector

            // The Jobs Subsector
            #region The Jobs Subsector

            this.subSector = "Employment";

            this.jobs = new Auxiliary(this, "jobs", 73, "persons")
            {
                UpdateFunction = delegate ()
                {
                    return potentialJobsInIndustrialSector.K + potentialJobsInAgriculturalSector.K + potentialJobsInServiceSector.K;
                }
            };

            this.potentialJobsInIndustrialSector = new Auxiliary(this, "potentialJobsInIndustrialSector", 74, "persons")
            {
                UpdateFunction = delegate () { return industrialCapital.K * jobsPerIndustrialCapitalUnit.K; }
            };

            this.jobsPerIndustrialCapitalUnit =
                new Table(this, "jobsPerIndustrialCapitalUnit", 75, "persons per dollar",
                    new double[] { 0.00037, 0.00018, 0.00012, 0.00009, 0.00007, 0.00006 }, 50, 800, 150)
                {
                    UpdateFunction = delegate () { return industrialOutputPerCapita.K; }
                };

            this.potentialJobsInServiceSector = new Auxiliary(this, "potentialJobsInServiceSector", 76, "persons")
            {
                UpdateFunction = delegate () { return serviceCapital.K * jobsPerServiceCapitalUnit.K; }
            };

            this.jobsPerServiceCapitalUnit =
                new Table(this, "jobsPerServiceCapitalUnit", 77, "persons per dollar",
                    new double[] { 0.0011, 0.0006, 0.00035, 0.0002, 0.00015, 0.00015 }, 50, 800, 150)
                {
                    UpdateFunction = delegate () { return serviceOutputPerCapita.K; }
                };

            this.potentialJobsInAgriculturalSector = new Auxiliary(this, "potentialJobsInAgriculturalSector", 78, "persons")
            {
                UpdateFunction = delegate () { return arableLand.K * jobsPerHectare.K; }
            };

            this.jobsPerHectare =
                new Table(this, "jobsPerHectare", 79, "persons per hectare",
                new double[] { 2, 0.5, 0.4, 0.3, 0.27, 0.24, 0.2, 0.2 }, 2, 30, 4)
                {
                    UpdateFunction = delegate () { return agriculturalInputsPerHectare.K; }
                };

            const double laborForceParticipationFraction = 0.75; // dimensionless
            this.laborForce = new Auxiliary(this, "laborForce", 80, "persons")
            {
                UpdateFunction = delegate ()
                {
                    return (population15To44.K + population45To64.K) * laborForceParticipationFraction;
                }
            };


            this.laborUtilizationFraction = new Auxiliary(this, "laborUtilizationFraction", 81)
            {
                UpdateFunction = delegate () { return jobs.K / laborForce.K; }
            };

            const double laborUtilizationFractionDelayedDelayTime = 2.0;    // years, eqn 82
            this.laborUtilizationFractionDelayed =
                new Smooth(
                    this, "laborUtilizationFractionDelayed", 82, "dimensionless",
                    laborUtilizationFractionDelayedDelayTime, "laborUtilizationFraction", 1.0);

            this.capitalUtilizationFraction =
                new Table(this, "capitalUtilizationFraction", 83, "dimensionless",
                    new double[] { 1.0, 0.9, 0.7, 0.3, 0.1, 0.1 }, 1, 11, 2)
                {
                    UpdateFunction = delegate ()
                    {
                        double value = laborUtilizationFractionDelayed.K;
                        return Value.IsAlmostZero(value) ? 1.0 : value;
                    }
                };

            #endregion The Jobs Subsector
        }
    }
}