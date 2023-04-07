import { GradeModel } from "../api/grades-client/grades-client.service"

export interface GradesViewModel {
    subject: string
    grades: GradeModel[]
    gradesPart1: GradeModel[]
    gradesPart2: GradeModel[]
    isRepresentative: boolean
    average: number | null
    percent: number | null
    weightsSum: number | null
}