using System.Text.Json.Serialization;

namespace Leebruce.Domain.Grades;

[JsonConverter( typeof( JsonStringEnumConverter ) )]
public enum SpecialGrade
{
	Plus,
	Minus,
	Unprepared,
}
