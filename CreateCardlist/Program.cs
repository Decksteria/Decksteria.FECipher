// See https://aka.ms/new-console-template for more information
using Decksteria.FECipher;
using FECipher;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

const string CommonDirectory = @"D:\Ernest's Folder";

const string OldJsonFile = @$"{CommonDirectory}\GitUpdate Repos\Multi-TCG Deck Builder Projects\FECipher\FECipher\cardlist.json";
const string CipherVitFileLocation = @$"{CommonDirectory}\Desktop Folders\Nothing Important\FECipherVit 5.9.0_en\res\cards\en";
const string NewJsonFile = $@"{CommonDirectory}\GitUpdate Repos\Decksteria\Decksteria.FECipher\Decksteria.FECipher\cardlist.json";

string jsonText = File.ReadAllText(OldJsonFile);
JsonElement jsonDeserialize = JsonSerializer.Deserialize<dynamic>(jsonText);
var jsonEnumerator = jsonDeserialize.EnumerateArray();

List<FECard> feCards = new List<FECard>();
foreach (var jsonCard in jsonEnumerator)
{
    string? id = jsonCard.GetProperty("CardID").GetString();
    string? character = jsonCard.GetProperty("Character").GetString();
    string? title = jsonCard.GetProperty("Title").GetString();
    string[]? colors = jsonCard.GetProperty("Color").Deserialize<string[]>();
    string? cost = jsonCard.GetProperty("Cost").GetString();
    JsonElement classChangeProperty;
    string? cccost = null;
    if (jsonCard.TryGetProperty("ClassChangeCost", out classChangeProperty))
    {
        cccost = classChangeProperty.GetString();
    }

    string? cardClass = jsonCard.GetProperty("Class").GetString();
    string[]? types = jsonCard.GetProperty("Type").Deserialize<string[]>();
    int minRange = jsonCard.GetProperty("MinRange").GetInt32();
    int maxRange = jsonCard.GetProperty("MaxRange").GetInt32();
    string? attack = jsonCard.GetProperty("Attack").GetString();
    string? support = jsonCard.GetProperty("Support").GetString();
    string? skill = jsonCard.GetProperty("Skill").GetString();
    JsonElement supportSkillProperty;
    string? supportSkill = null;
    if (jsonCard.TryGetProperty("SupportSkill", out supportSkillProperty))
    {
        supportSkill = supportSkillProperty.GetString();
    }

    string? rarity = jsonCard.GetProperty("Rarity").GetString();
    int seriesNo = jsonCard.GetProperty("SeriesNumber").GetInt32();
    var altArtEnumerator = jsonCard.GetProperty("AlternateArts").EnumerateArray();
    List<FEAlternateArts> altArts = new List<FEAlternateArts>();

    foreach (var altArt in altArtEnumerator)
    {
        string? code = altArt.GetProperty("CardCode").GetString();
        string? setNo = altArt.GetProperty("SetCode").GetString();
        string? image = altArt.GetProperty("ImageFile").GetString();
        string? lackeyID = altArt.GetProperty("LackeyCCGID").GetString();
        string? lackeyName = altArt.GetProperty("LackeyCCGName").GetString();
        string? cipherVitID = altArt.GetProperty("CipherVitId").GetString();
        string? imageURL = altArt.GetProperty("DownloadURL").GetString();

        //Cannot be Null
        if (code == null || setNo == null || image == null || lackeyID == null || lackeyName == null || cipherVitID == null || imageURL == null)
        {
            throw new ArgumentException("JSON Field AlternateArts is missing a Non-Nullable Property.");
        }

        FEAlternateArts alt = new FEAlternateArts(code, setNo, image, lackeyID, lackeyName, cipherVitID, imageURL.Trim());
        altArts.Add(alt);
    }

    if (id == null || character == null || title == null || colors == null || cost == null || cardClass == null || types == null ||
        attack == null || support == null || skill == null || rarity == null)
    {
        throw new ArgumentException(String.Format("JSON Object {0}: {1} is missing a Non-Nullabe Property.", character, title));
    }

    FECard card = new FECard(id, character, title, colors, cost, cccost, cardClass, types, minRange,
        maxRange, attack, support, skill, supportSkill, rarity, seriesNo, altArts);

    feCards.Add(card);
}

Console.WriteLine("Finished loading old FE Cipher JSON.");

var idMapping = new Dictionary<string, string>();
idMapping.Add("B04-017", "Tiki.MirageUta-loid");
idMapping.Add("B04-017X", "Tiki.MirageUta-loid.2");
idMapping.Add("B20-001", "CorrinKingdomofValla.MonarchForgingaNewFuture");
idMapping.Add("B20-001X", "CorrinKingdomofValla.MonarchForgingaNewFuture.2");
idMapping.Add("S04-003", "Camilla.BewitchingMaligKnight");
idMapping.Add("B11-101", "Camilla.BewitchingMaligKnight.2");

var downloadLocations = new Dictionary<string, string>();
downloadLocations.Add("B02-093R+", "https://s3-wiki.serenesforest.net/c/cc/B02-093%2B.png");
downloadLocations.Add("B03-047SR+", "https://s3-wiki.serenesforest.net/9/94/B03-047%2B.png");
downloadLocations.Add("B04-009R+", "https://s3-wiki.serenesforest.net/c/c2/B04-009%2B.png");
downloadLocations.Add("P14-009PRX", "https://s3-wiki.serenesforest.net/a/a6/P14-009X.png");
downloadLocations.Add("P16-009PRX", "https://s3-wiki.serenesforest.net/5/55/P16-009X.png");
downloadLocations.Add("B01-007ST", "https://s3-wiki.serenesforest.net/6/6b/B01-007ST.png");
downloadLocations.Add("B01-009ST", "https://s3-wiki.serenesforest.net/1/10/B01-009ST.png");

///*
var cipherVitFiles = Directory.GetFiles(CipherVitFileLocation, "*.fe0db");
foreach (var vitFile in cipherVitFiles)
{
    var fileLines = File.ReadAllLines(vitFile);
    foreach (var line in fileLines)
    {
        var fields = line.Split(',');

        var cipherVitId = fields[0].Trim();
        var set = fields[1].Trim();
        var initialCode = fields[2].Trim();
        var code = initialCode.Replace("X", "").Replace("ZZ", "");
        var fullname = fields[4].Split("%%");
        var character = fullname.First().Trim();
        var title = fullname.Last().Trim();
        var cost = fields[5].Trim();
        var cccost = fields[6].Trim();
        // var tier = fields[6];
        var cclass = fields[8].Trim();
        var colors = fields[9].Trim().Split('/');
        var gender = fields[10].Trim();
        var weapon = fields[11].Trim();
        var types = fields[12].Split('/').ToList();
        types.Add(gender.Trim());
        types.Add(weapon.Trim());
        types = types.Where(item => item != null && item != "-" && item != "None").ToList();
        var attack = fields[13].Trim();
        var support = fields[14].Trim();
        var range = fields[15] == "-" ? new[] { "0" } : fields[15].Split('-');
        int minRange = int.Parse(range.First().Trim());
        int maxRange = int.Parse(range.Last().Trim());
        var effect = fields[16].Replace("%%", ",").Replace("$$", "\n").Trim();
        var supportEffect = fields[17].Replace("%%", ",").Replace("$$", "\n").Trim();
        // var artcount = fields[18].Trim();
        var rarities = fields[19].Split('/');
        var arts = rarities.Select(rarity => $"{code}{rarity.Trim()}");

        if (character == "Azel")
        {
            character = "Azelle";
        }
        else if (character == "Claude (Jugdral)")
        {
            character = "Claud";
        }
        else if (character == "Manya")
        {
            character = "Annand";
        }
        else if (character == "Claude (Fodlan)")
        {
            character = "Claude";
        }
        else if (character == "Jerrot")
        {
            character = "Zelot";
        }
        else if (character == "Mamori")
        {
            character = "Mamori Minamoto";
        }
        else if (character == "Camilla")
        {
            if (title == "Bewitching Malig Knight" && set == "S04")
            {
                title = "Bewitching Knight";
            }
        }

        var matchingCards = feCards.Where(card => card.characterName == character && card.characterTitle == title);

        if (matchingCards.Count() == 0)
        {
            matchingCards = feCards.Where(card => card.characterName == character && card.altArts.Any(art => art.Id.Contains(code)));
        }

        if (matchingCards.Count() == 1)
        {
            var card = matchingCards.First();
            UpdateFECard(card, cost, cccost, cclass, colors, attack, support, minRange, maxRange, effect, supportEffect, types);
            foreach (var art in arts)
            {
                UpdateArts(card, cipherVitId, art, set);
                cipherVitId += "+";
            }
        }
        else if (matchingCards.Count() > 1)
        {
            var correctId = idMapping.GetValueOrDefault(initialCode);
            if (correctId == null)
            {
                Console.WriteLine($"Which of the following cards would {cipherVitId} - {initialCode} - {character} : {title} it be?\n{string.Join("\n", matchingCards.Select(art => art.ID))}");
                correctId = Console.ReadLine()?.Trim();
            }

            var correctCard = matchingCards.First(art => art.ID == correctId);
            UpdateFECard(correctCard, cost, cccost, cclass, colors, attack, support, minRange, maxRange, effect, supportEffect, types);
            foreach (var art in arts)
            {
                UpdateArts(correctCard, cipherVitId, art, set);
                cipherVitId += "+";
            }
        }
        else
        {
            Console.WriteLine($"Type in the ID of the card {cipherVitId} - {initialCode} - {character} {title}");
            var correctId = Console.ReadLine()?.Trim();
            var correctCard = feCards.First(card => card.ID == correctId);
            UpdateFECard(correctCard, cost, cccost, cclass, colors, attack, support, minRange, maxRange, effect, supportEffect, types);
            foreach (var art in arts)
            {
                UpdateArts(correctCard, cipherVitId, art, set);
                cipherVitId += "+";
            }
        }
    }
}
//*/

var options = new JsonSerializerOptions
{
    WriteIndented = true
};
string newJsonText = JsonSerializer.Serialize(feCards, options);
File.WriteAllText(OldJsonFile, newJsonText);

var newFECards = feCards.Select(card =>
{
    return new Decksteria.FECipher.Models.FECard
    {
        CardId = feCards.IndexOf(card),
        CharacterName = card.characterName,
        CardTitle = card.characterTitle,
        Colors = card.colors,
        Cost = card.cost,
        ClassChangeCost = card.classChangeCost,
        CardClass = card.cardClass,
        Types = card.types,
        MinRange = card.minRange,
        MaxRange = card.maxRange,
        Attack = card.attack,
        Support = card.support,
        Skill = card.skill,
        SupportSkill = card.supportSkill,
        AltArts = card.altArts.Select(art =>
        {
            var regex = new Regex(@"[A-Z][0-9]+\-[0-9]+");
            var rarity = regex.Replace(art.SetCode, "");
            return new Decksteria.FECipher.Models.FEAlternateArts
            {
                ArtId = card.altArts.IndexOf(art),
                CardCode = art.Id,
                SetCode = art.SetCode,
                Rarity = rarity,
                FileName = Path.GetFileName(art.ImageLocation),
                DownloadUrl = art.ImageDownloadURL,
                LackeyCCGId = art.LackeyCCGId,
                LackeyCCGName = art.LackeyCCGName,
                CipherVitId = art.CipherVitId,
                SeriesNo = Series(art.SetCode)
            };
        })
    };
});

newJsonText = JsonSerializer.Serialize(newFECards, options);
File.WriteAllText(NewJsonFile, newJsonText);

void UpdateFECard(FECard card, string cost, string cccost, string cclass, string[] colors, string attack, string support, int minRange, int maxRange, string effect, string supportEffect, IEnumerable<string> types)
{
    card.cost = cost ?? card.cost;
    card.classChangeCost = cccost ?? card.classChangeCost;
    card.cardClass = cclass ?? card.cardClass;
    card.colors = colors;
    card.attack = attack ?? card.attack;
    card.support = support ?? card.support;
    card.minRange = minRange;
    card.maxRange = maxRange;
    card.skill = effect ?? card.skill;
    card.supportSkill = supportEffect ?? card.supportSkill;

    if (card.types.Length != types.Count())
    {
        Console.WriteLine($"Which of the two types for {card.Name} is correct: [1] \"{string.Join('/', card.types)}\" or [2] \"{string.Join('/', types)}\"");
        if (Console.ReadLine()?.Trim() == "2")
        {
            card.types = types.ToArray();
        }
    }
}

void UpdateArts(FECard card, string cipherVitId, string CardCode, string SetCode)
{
    var matchingArts = card.altArts.Where(art => art.Id == CardCode);
    if (matchingArts.Any())
    {
        var art = matchingArts.First();
        art.CipherVitId = cipherVitId;
        art.SetCode = SetCode;
        var fileName = Path.GetFileName(art.ImageDownloadURL);
        art.ImageLocation = $@"\plug-ins\fe-cipher\images\{art.SetCode}\{fileName}";
    }
    else
    {
        if (card.altArts.Count == 1)
        {
            matchingArts = card.altArts;
        }
        else if (CardCode.EndsWith("ST"))
        {
            matchingArts = card.altArts.Where(art => !art.SetCode.StartsWith("P") && !art.Id.Contains("+"));
        }
        else
        {
            matchingArts = card.altArts.Where(art => art.SetCode == SetCode);
        }

        var firstArt = matchingArts.FirstOrDefault();
        if (matchingArts.Count() != 1)
        {
            Console.WriteLine($"Which of the following artworks would {cipherVitId} : {CardCode} be in LackeyCCG?\n{string.Join("\n", card.altArts.Select(art => art.LackeyCCGId))}");
            var lackeyid = Console.ReadLine()?.Trim();

            firstArt = card.altArts.First(art => art.LackeyCCGId == lackeyid);
        }

        var fileExtStart = firstArt!.ImageLocation.LastIndexOf('.');
        var fileExtension = firstArt.ImageLocation[fileExtStart..];
        var imageLocation = $@"\plug-ins\fe-cipher\images\{firstArt.SetCode}\{firstArt.Id}{fileExtension}";
        var downloadLocation = downloadLocations.GetValueOrDefault(CardCode);
        if (downloadLocation != null)
        {
            fileExtStart = downloadLocation.LastIndexOf('.');
            fileExtension = downloadLocation[fileExtStart..];
            imageLocation = $@"\plug-ins\fe-cipher\images\{firstArt.SetCode}\{CardCode}{fileExtension}";
        }
        else if (CardCode.EndsWith("ST"))
        {
            downloadLocation = firstArt.ImageDownloadURL;
        }
        else
        {
            Console.Write($"Location can card art {CardCode} - {card.Name} be downloaded from: ");
            downloadLocation = Console.ReadLine()!;
            fileExtStart = downloadLocation.LastIndexOf('.');
            fileExtension = downloadLocation[fileExtStart..];
            imageLocation = $@"\plug-ins\fe-cipher\images\{firstArt.SetCode}\{CardCode}{fileExtension}";
        }
        
        card.altArts.Add(new FEAlternateArts(CardCode, SetCode, imageLocation, firstArt.LackeyCCGId, firstArt.LackeyCCGName, cipherVitId, downloadLocation ?? ""));
    }
}

int Series(string setNo)
{
    switch (setNo)
    {
        case "B01":
        case "P01":
        case "S01":
        case "S02":
            return 1;
        case "B02":
        case "P02":
        case "S03":
        case "S04":
            return 2;
        case "B03":
        case "P03":
        case "S05":
            return 3;
        case "B04":
        case "P04":
        case "S06":
            return 4;
        case "B05":
        case "P05":
        case "S07":
            return 5;
        case "B06":
        case "P06":
        case "S08":
            return 6;
        case "B07":
        case "P07":
            return 7;
        case "B08":
        case "P08":
            return 8;
        case "B09":
        case "P09":
        case "S09":
            return 9;
        case "B10":
        case "P10":
            return 10;
        case "B11":
        case "P11":
            return 11;
        case "B12":
        case "P12":
            return 12;
        case "B13":
        case "P13":
        case "S10":
            return 13;
        case "B14":
        case "P14":
            return 14;
        case "B15":
        case "P15":
            return 15;
        case "B16":
        case "P16":
            return 16;
        case "B17":
        case "P17":
            return 17;
        case "B18":
        case "P18":
        case "S12":
            return 18;
        case "B19":
        case "P19":
            return 17;
        case "B20":
        case "P20":
            return 20;
        case "B21":
        case "P21":
            return 21;
        case "B22":
        case "P22":
            return 22;
    }

    return -1;
}