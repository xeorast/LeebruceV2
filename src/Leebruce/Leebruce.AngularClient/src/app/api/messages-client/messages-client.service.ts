import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, tap } from 'rxjs';
import { AuthenticationService, NotAuthenticatedError } from '../authentication/authentication.service';
import { HttpError, HttpProblem, ProblemDetails } from '../problem-details';

@Injectable()
export class MessagesClientService {

  constructor(
    private http: HttpClient,
    private auth: AuthenticationService ) { }

  public getMessages() {
    let authHeader = `Bearer ${this.auth.token}`;
    return this.http.get<MessageMetadataModel[]>( 'api/messages', { headers: { Authorization: authHeader } } )
      .pipe(
        tap( resp => resp.forEach( msg => msg.date = new Date( msg.date ) ) ),
        catchError( error => this.errorHandler( error ) )
      );
  }

  public getMessage( id: string ) {
    let authHeader = `Bearer ${this.auth.token}`;
    return this.http.get<MessageModel>( `api/messages/${id}`, { headers: { Authorization: authHeader } } )
      .pipe(
        tap( resp => resp.date = new Date( resp.date ) ),
        catchError( error => this.errorHandler( error ) )
      );
  }

  public downloadAttachment( id: string ) {
    let authHeader = `Bearer ${this.auth.token}`;
    return this.http.get( `api/messages/attachments/${id}`, { headers: { Authorization: authHeader } } )
      //todo: save file
      .pipe(
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

export interface MessageMetadataModel {
  subject: string,
  author: string,
  date: Date,
  hasAttachments: boolean
  id: string
}

export interface MessageModel {
  subject: string,
  author: string,
  characterClass: string,
  date: Date,
  content: string,
  attachments: AttachmentModel[]
}

export interface AttachmentModel {
  fileName: string,
  id: string,
}
