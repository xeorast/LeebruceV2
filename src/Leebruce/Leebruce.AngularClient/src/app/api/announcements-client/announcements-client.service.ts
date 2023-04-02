import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, tap } from 'rxjs';
import { AUTH_ENABLED_CONTEXT, NotAuthenticatedError } from '../authentication/authentication.service';
import { HttpError, HttpProblem, ProblemDetails } from '../problem-details';

@Injectable( /*{
  providedIn: 'root'
}*/ )
export class AnnouncementsClientService {

  constructor(
    private http: HttpClient ) { }

  public getAnnouncements() {
    let context = AUTH_ENABLED_CONTEXT
    return this.http.get<AnnouncementModel[]>( 'api/announcements', { context: context } )
      .pipe(
        tap( resp => resp.forEach( ann => ann.date = new Date( ann.date ) ) ),
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

export interface AnnouncementModel {
  title: string,
  date: Date,
  author: string,
  content: string
}