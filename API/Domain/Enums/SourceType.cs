using API.Helpers;

namespace API.Domain.Enums;

public enum SourceType
{
    [DescriptionAnnotation("Upload", "Upload")]
    Upload = 1,
    [DescriptionAnnotation("QRCode", "QRCode")]
    QRCode = 2,
    [DescriptionAnnotation("SMS", "SMS")]
    SMS = 3,    
    [DescriptionAnnotation("Google", "Google")]
    Google = 4,
    [DescriptionAnnotation("manual", "manual")]
    Manual = 5,
    [DescriptionAnnotation("Talabat", "Talabat")]
    Talabat = 6,
    [DescriptionAnnotation("Mrsool", "Mrsool")]
    Mrsool = 7,
    [DescriptionAnnotation("Hungerstation", "Hungerstation")]
    Hungerstation = 8,
    [DescriptionAnnotation("Keeta", "Keeta")]
    Keeta = 9,
    [DescriptionAnnotation("Instashop", "Instashop")]
    Instashop = 10,
    [DescriptionAnnotation("Elmenus", "Elmenus")]
    Elmenus = 11,
    [DescriptionAnnotation("Ninja","Ninja")]
    Ninja = 12,
    [DescriptionAnnotation("Noon","Noon")]
    Noon = 13,
    [DescriptionAnnotation("Careem","Careem")]
    Careem = 14,
}