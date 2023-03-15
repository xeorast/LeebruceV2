import { formatDate, Time } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, tap } from 'rxjs';
import { AuthenticationService, NotAuthenticatedError } from '../authentication/authentication.service';
import { HttpError, HttpProblem, ProblemDetails } from '../problem-details';

@Injectable()
export class TimetableClientService {

  constructor(
    private http: HttpClient,
    private auth: AuthenticationService ) { }

  public getTimetable() {
    let authHeader = `Bearer ${this.auth.token}`;
    return this.http.get<TimetableDayModel[]>( 'api/timetable', { headers: { Authorization: authHeader } } )
      .pipe(
        tap( resp => resp.forEach( day => day.date = new Date( day.date ) ) ),
        tap( resp => resp.forEach( day => this.convertLessonTimes( day ) ) ),
        catchError( error => this.errorHandler( error ) )
      );
  }

  public getTimetableForDate( date: Date ) {
    let authHeader = `Bearer ${this.auth.token}`;
    let dateStr = formatDate( date, 'yyyy-MM-dd', 'en-US' )
    return this.http.get<TimetableDayModel[]>( `api/timetable/${dateStr}`, { headers: { Authorization: authHeader } } )
      .pipe(
        tap( resp => resp.forEach( day => day.date = new Date( day.date ) ) ),
        tap( resp => resp.forEach( day => this.convertLessonTimes( day ) ) ),
        catchError( error => this.errorHandler( error ) )
      );
  }

  private convertLessonTimes( day: TimetableDayModel ) {
    for ( const lesson of day.lessons ) {
      if ( lesson ) {
        let startSegments = ( lesson.time.start as unknown as string ).split( ':' )
        let endSegments = ( lesson.time.end as unknown as string ).split( ':' )
        lesson.time.start = <Time>{ hours: +startSegments[0], minutes: +startSegments[1] }
        lesson.time.end = <Time>{ hours: +endSegments[0], minutes: +endSegments[1] }
      }
    }
  }

  private errorHandler( error: HttpError ): Observable<never> {
    let problemDetails: ProblemDetails | undefined
    if ( error instanceof HttpProblem ) {
      problemDetails = error.details
    }

    if ( error.response.status === 401 ) {
      throw new NotAuthenticatedError( problemDetails?.detail ?? undefined )
    }

    throw error
  }

}

export interface TimetableDayModel {
  date: Date,
  lessons: ( LessonModel | null )[]
}

export interface LessonModel {
  subject: string,
  teacherName: string,
  teacherSurname: string,
  group?: string,
  room?: string,
  time: LessonTimeModel,
  substitution?: SubstitutionModel,
  classAbsence: boolean,
  isCancelled: boolean
}

export interface LessonTimeModel {
  number: number,
  start: Time,
  end: Time
}

export interface SubstitutionModel {
  originalTeacher?: string,
  originalSubject?: string,
  originalRoom?: string,
}
