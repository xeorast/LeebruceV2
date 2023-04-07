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
    return this.http.get<GradesDataModel>( 'api/grades', { context: context } )
      .pipe(
        tap( GradesClientService.convertDatesInResp )
      );
  }

  public getNewGrades() {
    let context = AUTH_ENABLED_REDIRECT_TO_LOGIN_CONTEXT
    return this.http.get<GradesDataModel>( 'api/grades/new', { context: context } )
      .pipe(
        tap( GradesClientService.convertDatesInResp )
      );
  }

  static convertDatesInResp( resp: GradesDataModel ) {
    resp.subjects.forEach(
      sub => {
        sub.firstTermGrades.forEach( GradesClientService.convertDatesInGrade )
        sub.secondTermGrades.forEach( GradesClientService.convertDatesInGrade )
      } )
  }

  static convertDatesInGrade( grade: GradeModel ) {
    grade.date = new Date( grade.date )
  }

}

export interface GradesDataModel {
  isByPercent: boolean
  subjects: SubjectGradesModel[]
}

export interface SubjectGradesModel {
  subject: string
  firstTermGrades: GradeModel[]
  secondTermGrades: GradeModel[]
  isRepresentative: boolean
  average: number | null
  percent: number | null
}

export interface GradeModel {
  value?: number
  specialValue?: SpecialGrade
  verySpecialValue?: SpecialGrade
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
