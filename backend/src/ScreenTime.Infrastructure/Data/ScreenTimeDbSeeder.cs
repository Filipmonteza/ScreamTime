using ScreenTime.Domain.Entities;

namespace ScreenTime.Infrastructure.Data;

public static class ScreenTimeDbSeeder
{
    public static List<Movie> GetMovies()
    {
        return new List<Movie>
        {
            new Movie
            {
                Title = "Exorcisten",
                AgeRating = 0,
                RuntimeMinutes = 122,
                Description = "I ett lugnt stadsområde drabbas en ung flicka av en skrämmande, mystisk åkomma. Hennes mor, en berömd filmstjärna, står maktlös när varken läkare eller psykiatriker kan förklara vad som händer. Till slut återstår bara ett desperat val: att vända sig till kyrkan.",
                IsActive = true
            },
            new Movie
            {
                Title = "Hereditary",
                AgeRating = 0,
                RuntimeMinutes = 126,
                Description = "När familjen Grahams matriark Ellen dör börjar hennes dotters familj att uppdaga kryptiska och allt mer skrämmande hemligheter om familjens historia. Ju mer de upptäcker, desto mer tvingas de att försöka undvika det olycksaliga öde som verkar vara deras arvegods.",
                IsActive = true
            },
            new Movie
            {
                Title = "The Shining",
                AgeRating = 0,
                RuntimeMinutes = 120,
                Description = "Jack Torrance är en nykter alkoholist som tar jobb som vaktmästare på det isolerade hotellet Overlook under vintersäsongen. Med sig på resan har han sin fru och sin lilla son. Jack hoppas på att få lugn och ro när han skriver sin nya bok. Men vad han inte vet är att hotellet har en ond och mörk hemlighet. Och något hemskt väntar på dem i rum 237…",
                IsActive = true
            },
            new Movie
            {
                Title = "Psycho",
                AgeRating = 0,
                RuntimeMinutes = 109,
                Description = "För att ha råd att kunna gifta sig, flyr kontoristen Marion Crane i tillfällig sinnesförvirring från Phoenix i sin bil efter att ha stulit 40 000 dollar som hennes arbetsgivare uppdragit åt henne att sätta in på banken. Beslutet leder henne till att svänga förbi ett ödsligt motell, som sköts av en viss Norman Bates och hans mor. Till det yttre verkar han vara en rar och väldigt blyg man. Men Norman Bates bär på en mörk hemlighet...",
                IsActive = true
            },
            new Movie
            {
                Title = "The Thing",
                AgeRating = 0,
                RuntimeMinutes = 109,
                Description = "Vintern 1982 på en isolerad forskningsstation i Antarktis upptäcker en forskargrupp något främmande som legat begravt i isen i över 100 000 år. När det väcks till liv följer en kedja av förödande händelser som snabbt gör att gruppen tappar greppet om verkligheten – och om varandra.",
                IsActive = true
            },
            new Movie
            {
                Title = "Casper",
                AgeRating = 0,
                RuntimeMinutes = 101,
                Description = "Spökdoktorn James Harvey och hans dotter Kat anländer till det gamla dragiga slottet Whipstaff Manor. Dess girige ägare, Carrigan Crittendon, har anlitat dr Harvey för att driva ut slottets osynliga invånare: ett vänligt men ensamt spöke som heter Casper, vars enda önskan är att finna en vän, och hans skandalösa farbröder, Stretch, Stinkie och Fatso (\"Spöktrion\"). Om planen går i lås kommer Carrigan och Dibs att kunna lägga vantarna på slottets mytomspunna skatt. Under tiden har Casper funnit en vän i Kat, men Spöktrion accepterar inga \"köttsliga\" i sitt hus…",
                IsActive = true
            },
            new Movie
            {
                Title = "The Conjuring",
                AgeRating = 0,
                RuntimeMinutes = 112,
                Description = "Ed och Lorraine Warren är berömda utredare av paranormala fenomen. De blir ombedda att hjälpa en familj som blir terroriserade av en okänd kraft i deras hus på landsbygden.",
                IsActive = true
            },
            new Movie
            {
                Title = "Alien",
                AgeRating = 0,
                RuntimeMinutes = 117,
                Description = "När besättningen på rymdskeppet Nostromo är på väg hem till jorden efter ett uppdrag får de plötsligt en nödsignal från en rymdstation på närliggande planet. När de landat inser de att något inte stämmer. Besättnignen har ingen aning om vilken mardröm de landat i.",
                IsActive = true
            },
            new Movie
            {
                Title = "Alla Helgons Blodiga Natt",
                AgeRating = 0,
                RuntimeMinutes = 91,
                Description = "Den 31 oktober 1963 tar den sexåriga Michael Myers på sig en mask och mördar med kniv sin 17-åriga syster. Han hamnar på mentalsjukhus och rymmer 15 år senare på väg till en ny anstalt, och beger sig hem till Haddonfield, där han på nytt börjar mörda.",
                IsActive = true
            },
            new Movie
            {
                Title = "Sinister",
                AgeRating = 0,
                RuntimeMinutes = 110,
                Description = "Hittade inspelningar hjälper en författare som skriver om verkliga brott att förstå hur och varför en familj mördades i det hus han flyttat in i. Men hans upptäckt gör att han och hans hela familj blir måltavlan för ett övernaturligt väsen.",
                IsActive = true
            }
        };
    }
}
