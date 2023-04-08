import { GradeModel } from "../api/grades-client/grades-client.service"

export interface GradesViewModel {
    isByPercent: boolean
    subjects: SubjectGradesViewModel[]
}

export interface SubjectGradesViewModel {
    subject: string
    firstTermGrades: GradeModel[]
    secondTermGrades: GradeModel[]
    newGrades: NewGradesModel
    isRepresentative: boolean
    average?: number
    percent?: number
    weightsSum?: number
}

export interface NewGradesModel {
    firstTerm: GradeModel[]
    secondTerm: GradeModel[]
}
