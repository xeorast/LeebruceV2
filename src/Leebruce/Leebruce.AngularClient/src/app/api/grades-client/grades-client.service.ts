import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { tap } from 'rxjs';
import { AUTH_ENABLED_REDIRECT_TO_LOGIN_CONTEXT } from '../authentication/authentication.service';

@Injectable()
export class GradesClientService {

  constructor(
    private http: HttpClient ) { }

  public getGrades() {
    let context = AUTH_ENABLED_REDIRECT_TO_LOGIN_CONTEXT
    return this.http.get<SubjectGradesModel[]>( 'api/grades', { context: context } )
      .pipe(
        tap( resp => resp.forEach(
          sub => sub.grades.forEach(
            grade => grade.date = new Date( grade.date )
          ) ) )
      );
  }

}

export interface SubjectGradesModel {
  subject: string
  grades: GradeModel[]
  isRepresentative: boolean
}

export interface GradeModel {
  value?: number
  specialValue?: SpecialGrade
  countToAverage: boolean
  weight?: number
  category: string
  comment?: string
  date: Date
  teacher: string
  addedBy: string
  colorHex: string
}

export type SpecialGrade = "Plus" | "Minus" | "Unprepared"
