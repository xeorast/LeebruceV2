namespace Leebruce.Domain;

public record class CollectionPage<T>(
	int TotalPages,
	int CurrentPage,
	T[] Elements );
