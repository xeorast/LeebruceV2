import { formatDate } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, tap } from 'rxjs';
import { AUTH_ENABLED_CONTEXT, NotAuthenticatedError } from '../authentication/authentication.service';
import { HttpError, HttpProblem, ProblemDetails } from '../problem-details';

@Injectable()
export class ScheduleClientService {

  constructor(
    private http: HttpClient ) { }

  public getSchedule() {
    let context = AUTH_ENABLED_CONTEXT
    return this.http.get<ScheduleDayModel[]>( 'api/schedule', { context: context } )
      .pipe(
        tap( resp => resp.forEach( day => day.day = new Date( day.day ) ) ),
        tap( resp => resp.forEach( day => {
          for ( const event of day.events ) {
            if ( event.dateAdded ) {
              event.dateAdded = new Date( event.dateAdded )
            }
          }
        } ) ),
        catchError( error => this.errorHandler( error ) )
      );
  }

  public getScheduleForDate( date: Date ) {
    let context = AUTH_ENABLED_CONTEXT
    let dateStr = formatDate( date, 'yyyy-MM-dd', 'en-US' )
    return this.http.get<ScheduleDayModel[]>( `api/schedule/${dateStr}`, { context: context } )
      .pipe(
        tap( resp => resp.forEach( day => day.day = new Date( day.day ) ) ),
        tap( resp => resp.forEach( day => {
          for ( const event of day.events ) {
            if ( event.dateAdded ) {
              event.dateAdded = new Date( event.dateAdded )
            }
          }
        } ) ),
        catchError( error => this.errorHandler( error ) )
      );
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

export interface ScheduleDayModel {
  day: Date,
  events: ScheduleEventModel[]
}

export interface ScheduleEventModel {
  id?: string,
  dateAdded?: Date,
  absenceData?: AbsenceDataModel,
  cancellationData?: CancellationDataModel,
  classAbsenceData?: ClassAbsenceDataModel,
  freeDayData?: FreeDayData,
  substitutionData?: SubstitutionDataModel,
  testEtcData?: TestEtcDataModel,
  unrecognizedData?: UnrecognizedDataModel,
  error?: string
}

export interface AbsenceDataModel {
  who: string,
  characterClass: string,
  time?: TimePair,
}

export interface TimePair {
  start: string,
  end: string,
}

export interface CancellationDataModel {
  who: string,
  subject: string,
  lessonNo: number,
}

export interface ClassAbsenceDataModel {
  class: string,
  when: string,
}

export interface FreeDayData {
  who: string,
  what: string,
}

export interface SubstitutionDataModel {
  who: string,
  subject: string,
  lessonNo: number,
}

export interface TestEtcDataModel {
  subject?: string,
  creator?: string,
  what?: string,
  description?: string,
  lessonNo?: number,
  room?: string,
  group?: string,
}

export interface UnrecognizedDataModel {
  value?: string,
}
