export interface CollectionPage<T> {
    totalPages: number
    currentPage: number
    elements: T[]
}
