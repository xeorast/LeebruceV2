import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { AuthenticationService, NotAuthenticatedError } from '../authentication/authentication.service';
import { HttpError, HttpProblem, ProblemDetails } from '../problem-details';

@Injectable( /*{
  providedIn: 'root'
}*/ )
export class AnnouncementsClientService {

  constructor(
    private http: HttpClient,
    private auth: AuthenticationService ) { }

  public getAnnouncements() {
    let authHeader = `Bearer ${this.auth.token}`;
    return this.http.get<AnnouncementModel[]>( 'api/announcements', { headers: { Authorization: authHeader } } )
      .pipe(
        catchError( this.errorHandler )
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