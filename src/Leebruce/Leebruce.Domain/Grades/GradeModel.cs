using System.Text.Json.Serialization;

namespace Leebruce.Domain.Grades;

public enum SpecialGrade
{
	Plus,
	Minus,
	Unprepared,
}

public record GradeModel(
	int? Value,
	SpecialGrade? SpecialValue,
	bool CountToAverage,
	int? Weight,
	string Category,
	string? Comment,
	DateOnly Date,
	string Teacher,
	string AddedBy,
	string ColorHex )
{
	[JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
	public int? Value { get; init; } = Value;

	[JsonConverter( typeof( JsonStringEnumConverter ) )]
	[JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
	public SpecialGrade? SpecialValue { get; init; } = SpecialValue;

	[JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
	public int? Weight { get; init; } = Weight;

	[JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
	public string? Comment { get; init; } = Comment;

}