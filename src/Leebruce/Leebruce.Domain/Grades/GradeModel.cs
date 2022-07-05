using System.Text.Json.Serialization;

namespace Leebruce.Domain.Grades;

public enum SpecialGrade
{
	Plus,
	Minus,
	Unprepared,
}

public record GradeModel(
	string? Id,
	int? Value,
	SpecialGrade? SpecialValue,
	bool CountToAverage,
	int? Weight,
	string Category,
	DateOnly Date,
	string Teacher,
	string AddedBy )//todo: add comment field
{
	[JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
	public int? Value { get; init; } = Value;

	[JsonConverter( typeof( JsonStringEnumConverter ) )]
	[JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
	public SpecialGrade? SpecialValue { get; init; } = SpecialValue;

	[JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
	public int? Weight { get; init; } = Weight;

}