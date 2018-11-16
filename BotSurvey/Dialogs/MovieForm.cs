using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BotSurvey.Dialogs
{
    [Serializable]
    public class MovieForm
    {
        [Prompt("Quel est votre adresse email ? {||}")]
        [Pattern("(@)(.+)$")]
        public string Email;
        [Prompt("Voulez-vous recevoir notre newsletter ? {||}")]
        public bool Newsletter;
        public LocationTheaterOptions LocationTheater;
        public MovieCategoryOptions MovieCategory;
        public DrinkOptions Drink;
        [Numeric(10, 100)]
        public int Age;

        public LanguageOptions Language;


        public static IForm<MovieForm> BuildForm()
        {
            return new FormBuilder<MovieForm>()
                    .Message("Merci de prendre quelques minutes pour répondre aux questions.")
                        
                        .Field(new FieldReflector<MovieForm>(nameof(LocationTheater))
                        //permet de skipper des questoins en fonction d'une réponse 
                            .SetNext(SetNextAfterLocation))
                        .Field(nameof(Drink), state => state.LocationTheater == LocationTheaterOptions.Paris13)
                        .Field(nameof(Age))
                        .Field(nameof(MovieCategory), validate: async (state, value) =>
                        {
                            var category = (MovieCategoryOptions)value;

                            var result = new ValidateResult() { IsValid = true, Value = category };

                            if (state.Age <= 12 && category == MovieCategoryOptions.Horror)
                            {
                                result.IsValid = false;
                                result.Feedback = "Vous n'êtes pas assez agé pour voir ce type de film";
                            }

                            return result;
                          })

                        .Field(nameof(Language))
                        // rend active la question en fonction du state
                        .Field(nameof(Newsletter))

                        .Field(nameof(Email), state => state.Newsletter)
                    .Confirm("Est-ce votre selection ? {*}")

                    .Build();
        }

        private static NextStep SetNextAfterLocation(object value, MovieForm state)
        {

            switch ((LocationTheaterOptions)value)
            {
                case LocationTheaterOptions.Paris13:
                    return new NextStep(new[] { nameof(Drink) });
                case LocationTheaterOptions.Paris12:
                    return new NextStep();

                case LocationTheaterOptions.Autre:
                    return new NextStep(StepDirection.Quit);

                default:
                    return new NextStep();

            }
        }


    }
    // obliger de mettre le premier enum à 1 car le 0 correspond au zéro choix
    public enum LocationTheaterOptions { Paris13 = 1, Paris12, Autre };
    public enum MovieCategoryOptions {
        
        [Describe("Science-fiction")]
        [Terms("SF", "Science fiction", "Science-fiction")]
        ScienceFiction = 1,
        Adventure,
        Horror,
        Autre };
    public enum DrinkOptions {
        [Describe(image: "https://www.staples.fr/content/images/product/76170-00H_1_xnl.jpg")]
        Eau = 1,
        [Describe(image: "https://www.staples.fr/content/images/product/72255-00H_1_xnl.jpg")]
        Coca,
        Rien }
    public enum LanguageOptions { French = 1, VO };

}