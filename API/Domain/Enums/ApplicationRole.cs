using API.Helpers;

namespace API.Domain.Enums
{
    public enum ApplicationRole
    {
        [DescriptionAnnotation("ADMIN", "ADMIN")]
        ADMIN = 1,

        [DescriptionAnnotation("LOCATION MANAGER", "LOCATION MANAGER")]
        LOCATION_MANAGER = 2,

  
        [DescriptionAnnotation("GROUP_MANAGER", "GROUP_MANAGER")]
        GROUP_MANAGER = 3,


        [DescriptionAnnotation("MODERATOR", "MODERATOR")]
        Moderator = 4,


        

  
    }
}
