using DataAccess.Entities;
using CustomExceptions;
using Models;
using System.Data.SqlClient;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DataAccess;

public class InteractionRepo : Interactions
{
    private readonly wearelosingsteamContext _context;
    private readonly IUserDAO _userRepo;
    private readonly ICompanionDAO _compRepo;
    private readonly IResourceGen _ResourceRepo;
    
    public InteractionRepo(wearelosingsteamContext context, IUserDAO userRepo, ICompanionDAO compRepo, IResourceGen ResourceRepo)
    {
        _context = context;
        _userRepo = userRepo;
        _compRepo = compRepo;
        _ResourceRepo = ResourceRepo;
    }    

    /// <summary>
    /// Method that modify the mood value of a particular companion
    /// </summary>
    /// <param name="companionID"></param>
    /// <param name="amount"></param>
    public bool SetCompanionMoodValue(int companionID, int amount)
    {
        Companion companionToDepress = _context.Companions.Find(companionID);  //Get comp followed by checkifnull
        if(companionToDepress == null)
        {
            throw new CompNotFound();
        }

        companionToDepress.Mood = companionToDepress.Mood + amount; //adjust mood based on determined amount

        if(companionToDepress.Mood <= 0)
        {
            companionToDepress.Mood = 0; //preventing negative numbers
        }

        companionToDepress.TimeSinceLastChangedMood = DateTime.Now;              //resetting the mood timer on the companion

        _context.SaveChanges();

        _context.ChangeTracker.Clear();

        return true;
    }

    public EmotionChart GetEmotionByEmotionId(int emotionId)
    {
        return _context.EmotionCharts.FirstOrDefault(emotionToBeFound => emotionToBeFound.EmotionId == emotionId) ?? throw new ResourceNotFound("No emotion with this ID exists.");
    }

    /// <summary>
    /// Method that modify the hunger value of a particular companion
    /// </summary>
    /// <param name="companionID"></param>
    /// <param name="amount"></param>
    public bool SetCompanionHungerValue(int companionID, int amount)
    {
        Companion companionToStarve = _context.Companions.Find(companionID);  //Retrieve companion object from database by the given CompanionID
        if(companionToStarve == null)
        {
            throw new ResourceNotFound();
        }
        companionToStarve.Hunger = companionToStarve.Hunger + amount;//Modify the hunger value
        companionToStarve.TimeSinceLastChangedHunger = DateTime.Now;
        _context.SaveChanges();//save changes
        _context.ChangeTracker.Clear();
        return true;
    }

    /// <summary>
    /// Rolls from a pool of possible moods and assigns it to a companion
    /// </summary>
    /// <param name="companionID"></param>
    /// <param name="amount"></param>
    public bool RollCompanionEmotion(int companionID, int emotionId)
    {
        Companion companionEmotionToSet = _context.Companions.Find(companionID); //Grabbing the companion
        if(companionEmotionToSet == null)                                                   //Checking null
        {
            throw new CompNotFound();
        }

        companionEmotionToSet.Emotion = emotionId; //You can get less than 0 and greater than 10 but I figure we will figure this out together

        _context.SaveChanges();

        _context.ChangeTracker.Clear();

        return true;
    }

    /// <summary>
    /// Allow a user to feed a particular type of food to a particular companion
    /// </summary>
    /// <param name="feederID"></param>
    /// <param name="companionID"></param>
    /// <param name="foodID"></param>
    /// <returns></returns>
    public bool FeedCompanion(int feederID, int companionID, int foodID)
    {
        Companion companionToStarve = _context.Companions.Find(companionID);  //Retrieve companion object from database by the given CompanionID
        User user2Check = _context.Users.Find(feederID); //Retrieve user object from database by the given FeederID
        FoodStat food2Feed = _context.FoodStats.Find(foodID); //Retrieve foodStats object from database by the given CompanionID
        Species species2check = _context.Species.Find(companionToStarve.SpeciesFk); //Retrieve Species object from database by the given CompanionID
        Random RNGjesusManifested = new Random();  
        if(companionToStarve == null || user2Check == null || food2Feed == null || species2check==null)
        {
            throw new ResourceNotFound();
        }
        if(companionToStarve.Hunger>90)
        {
            throw new TooSoon("Your buddy ain't hungy yet!");
        }
        
        bool match = (species2check.FoodElementIdFk == food2Feed.FoodStatsId);
        int baseAmountHunger = 0; //neither of these numbers make any damm sense
        int baseAmountMood = 0; 
        if(match)
        {
            baseAmountHunger = RNGjesusManifested.Next(25,31);
            baseAmountMood = RNGjesusManifested.Next(25,31);
        }
        else
        {
            baseAmountHunger = RNGjesusManifested.Next(-15,31);
            baseAmountMood = RNGjesusManifested.Next(-15,31);
        }

        double HungerModifier = 1;
        double MoodModifier = 1;
        if(match) //I know all of these can be compress into the if else block above, I am keeping them seperated for my own sanity sake, STFU
        {
            HungerModifier = HungerModifier + RNGjesusManifested.NextDouble();
            MoodModifier = MoodModifier + RNGjesusManifested.NextDouble();
            if(HungerModifier>1.90) //great success
            {
                HungerModifier = HungerModifier + (RNGjesusManifested.NextDouble()*0.2);
            }
            if(MoodModifier>1.90) //great success
            {
                MoodModifier = MoodModifier + (RNGjesusManifested.NextDouble()*0.2);
            }
            if(MoodModifier>1.95 && HungerModifier>1.95) //critical success! It also makes your companion invincible for two days when we get to the dungeon crawling feature
            {
                HungerModifier = HungerModifier + (RNGjesusManifested.NextDouble()*0.1);
                MoodModifier = MoodModifier + (RNGjesusManifested.NextDouble()*0.1);
            }
        }
        else
        {
            HungerModifier = HungerModifier - RNGjesusManifested.NextDouble();
            MoodModifier = MoodModifier - RNGjesusManifested.NextDouble();
            if(HungerModifier<0.15) //great failure
            {
                HungerModifier = HungerModifier - (RNGjesusManifested.NextDouble()*0.2);
            }
            if(MoodModifier<0.15) //great failure
            {
                MoodModifier = MoodModifier - (RNGjesusManifested.NextDouble()*0.2);
            }
            if(MoodModifier<0.05 && HungerModifier<0.05) //critical failure! It also makes your companion deal 50% less damage for the next two days
            {
                HungerModifier = HungerModifier - (RNGjesusManifested.NextDouble()*0.1);
                MoodModifier = MoodModifier - (RNGjesusManifested.NextDouble()*0.1);
            }
        }

        int moodAmount = 0;
        int hungerAmount = 0;
        if(match) //I know all of these can be compress into the if else block above, I am keeping them seperated for my own sanity sake, STFU
        {
            moodAmount = (int)Math.Ceiling(baseAmountMood*MoodModifier);
            hungerAmount = (int)Math.Ceiling(baseAmountHunger*HungerModifier);
        }
        else
        {
            moodAmount = (int)Math.Floor(baseAmountMood*MoodModifier);
            hungerAmount = (int)Math.Floor(baseAmountHunger*HungerModifier);
        }

        try
        {
            SetCompanionHungerValue(companionID,hungerAmount);
            SetCompanionMoodValue(companionID,moodAmount);
        }
        catch(Exception)
        {
            throw;
        }

        try
        {
            _ResourceRepo.RemoveFood(feederID,foodID); //last step
            return true; //operation success
        }
        catch(Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Allows a user to pet a companion, changing the said companion's mood base on their hunger level
    /// </summary>
    /// <param name="petterID"></param>
    /// <param name="companionID"></param>
    /// <returns></returns>
    public bool PetCompanion(int userID, int companionID)//limit petting to every 3-5min
    {
        Random agitationRoll = new Random();             //For our random roll later

        Companion companionToPet = _compRepo.GetCompanionByCompanionId(companionID); //grabbing the companion
        if(companionToPet == null)                                                   //checking null
        {
            throw new CompNotFound();
        }

        User userToPet = _userRepo.GetUserByUserId(userID);  //grabbing the user
        if(userToPet == null)                                //checking null
        {
            throw new UserNotFound();
        }

        int moodToOffset = 0;

        int hungerMod = 0;       //this value will modify the chance for companion agitation based on hunger
        if(companionToPet.Hunger <= 15)
        {
            hungerMod = -30;        //"roll a agitation threshold based on hunger (if the pet is hungry, the agitation threshold should be weighted to roll high)"
        }
        else if(companionToPet.Hunger <= 35)
        {
            hungerMod = -20;
        }
        else if(companionToPet.Hunger <= 60)
        {
            hungerMod = -10;
        }        
        else if(companionToPet.Hunger <= 75)
        {
            hungerMod = -5;
        }
        else if(companionToPet.Hunger >= 90)
        {
            hungerMod = 0;
        }                  

        int moodMod = 0;       //this value will modify the chance for companion agitation based on mood
        if(companionToPet.Mood <= 15)
        {
            moodMod = -5;       
        }
        else if(companionToPet.Mood <= 35)
        {
            moodMod = 0;
        }
        else if(companionToPet.Mood <= 60)
        {
            moodMod = 5;
        }        
        else if(companionToPet.Mood <= 75)
        {
            moodMod = 15;
        }
        else if(companionToPet.Mood >= 90)
        {
            moodMod = 30;
        }        

        bool companionShowcase = false;            //Setting this to check if showcase companion
        int showcaseMod = 0;                       //If bool = true, check gets +10 to succeed on agitation threshold check
        if(userToPet.ShowcaseCompanionFk == companionToPet.CompanionId)//Checking whether it is or not
        {
            companionShowcase = true;     
        }

        if(companionShowcase == true)              //Setting Bonus if true
        {
            showcaseMod = 10;
        }

        int agitationBaseRoll = agitationRoll.Next(15, 35);//Rolling base roll with previously set random number generator

        int totalRoll = agitationBaseRoll + hungerMod + moodMod + showcaseMod;

        if(totalRoll < 50)
        {
            if(companionToPet.Mood <= 15)
            {
                moodToOffset = -7; //This is a weird one.... because the number is already soooo low it'll probably hit 0 or close to it anyway...      
            }
            else if(companionToPet.Mood <= 35)
            {
                moodToOffset = -20;     //This number is bigger than the above one because if it is agitated with a low mood we want the change obvious
            }
            else if(companionToPet.Mood <= 60)
            {
                moodToOffset = -10;     //Sucks to suck and it is noticable but not too bad 
            }        
            else if(companionToPet.Mood <= 75)
            {
                moodToOffset = 0;       //These numbers are pretty harsh but also I weighed it very likely for petting to be a positive result.
            }
            else if(companionToPet.Mood >= 90)
            {
                moodToOffset = 5;       //I mean does it really need to be much happier?
            }  
        }
        else if(totalRoll >= 50)
        {
            if(companionToPet.Mood <= 15)
            {
                moodToOffset = 30; //Give them a big bonus because it could be risky and they need it the most (we could make this a random range too if we want in theory)      
            }
            else if(companionToPet.Mood <= 35)
            {
                moodToOffset = 20;     //This number is bigger than the above one because if it is agitated with a low mood we want the change obvious
            }
            else if(companionToPet.Mood <= 60)
            {
                moodToOffset = 15;     //Numbers becoming less since the companion is already in a relatively good mood. Obvi we can change them.
            }        
            else if(companionToPet.Mood <= 75)
            {
                moodToOffset = 10;       
            }
            else if(companionToPet.Mood >= 90)
            {
                moodToOffset = 5;       //I mean does it really need to be much happier?
            } 
        }

        companionToPet.Mood = companionToPet.Mood + moodToOffset; //I think rolling for agitation is good, but the actual numbers may wanna be changed in the end.

        if(companionToPet.Mood <= 0) //preventing negatives and values over 100
        {
            companionToPet.Mood = 0;
        }
        if(companionToPet.Mood >= 100)
        {
            companionToPet.Mood = 100;
        }

        companionToPet.TimeSinceLastPet = DateTime.Now;            

        _context.SaveChanges(); //Maybe this method could also have a percentage change to reroll the emotion? A greater chance to change emotion if mood is low or emotion quality is poor

        _context.ChangeTracker.Clear();

        return true;
    }

    /// <summary>
    /// Changing the showcased companion field of a perticular user
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="companionID"></param>
    /// <returns></returns>
    public bool ShowCaseCompanion(int userID, int companionID)
    {
        //Retrieve companion object from database by the given CompanionID
        //Retrieve user object from database by the given userID

        //Set the showcase companion value in the user table to the given companionID 
        //return true after successful operation  
        return false;
    }

    public string PullConvo(int CompanionID)
    {
        string returnString = "Network error, go bother your ISP";
        Companion companionToTalk = _context.Companions.Find(CompanionID);  //Retrieve companion object from database by the given CompanionID

        IEnumerable<Conversation> checkForSpecies = //copped this code whole sale from FriendsRepo
            (from Conversation in _context.Conversations
            where (Conversation.SpeciesFk == companionToTalk.SpeciesFk)
            select Conversation);
        List<Conversation> friendsList = checkForSpecies.ToList(); //Retrieve A list of conversation that matches the given species.

        Random RNGjesusManifested = new Random();  
        //Pull from that list, ONE random conversation based on the mood of the companion
        //If the companion has a high mood value, it should be more likely that a high quality conversation gets chosen
        //return the conversation as string

        return returnString;
    }
}