import { formatDate, Time } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { tap } from 'rxjs';
import { AUTH_ENABLED_CONTEXT } from '../authentication/authentication.service';

@Injectable()
export class TimetableClientService {

  constructor(
    private http: HttpClient ) { }

  public getTimetable() {
    let context = AUTH_ENABLED_CONTEXT
    return this.http.get<TimetableDayModel[]>( 'api/timetable', { context: context } )
      .pipe(
        tap( resp => resp.forEach( day => day.date = new Date( day.date ) ) ),
        tap( resp => resp.forEach( day => this.convertLessonTimes( day ) ) )
      );
  }

  public getTimetableForDate( date: Date ) {
    let dateStr = formatDate( date, 'yyyy-MM-dd', 'en-US' )
    let context = AUTH_ENABLED_CONTEXT
    return this.http.get<TimetableDayModel[]>( `api/timetable/${dateStr}`, { context: context } )
      .pipe(
        tap( resp => resp.forEach( day => day.date = new Date( day.date ) ) ),
        tap( resp => resp.forEach( day => this.convertLessonTimes( day ) ) )
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
