import { GradeModel } from "../api/grades-client/grades-client.service"

export interface GradesViewModel {
    subject: string
    firstTermGrades: GradeModel[]
    secondTermGrades: GradeModel[]
    isRepresentative: boolean
    average?: number
    percent?: number
    weightsSum?: number
}