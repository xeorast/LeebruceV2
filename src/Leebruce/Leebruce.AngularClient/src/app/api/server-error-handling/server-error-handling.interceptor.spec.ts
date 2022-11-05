import { HttpErrorResponse, HttpEvent, HttpHandler, HttpRequest } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { Observer, Subject } from 'rxjs';
import { ErrorNotifierService, ProxyError, ProxyTimeoutError, ServerError, TechnicalBreakError } from '../error-notifier/error-notifier.service';
import { HttpError, HttpProblem, ProblemDetails } from '../problem-details';

import { ServerErrorHandlingInterceptor } from './server-error-handling.interceptor';

describe( 'ServerErrorHandlingInterceptor', () => {
  let interceptor: ServerErrorHandlingInterceptor;
  let errorNotifierService: ErrorNotifierService;

  const nextSubject = new Subject<any>();
  const next: HttpHandler = {
    handle: () => {
      return nextSubject.asObservable();
    }
  };

  beforeEach( () => {
    TestBed.configureTestingModule( {
      providers: [
        ServerErrorHandlingInterceptor,
        {
          provide: ErrorNotifierService,
          useValue: jasmine.createSpyObj( typeof ErrorNotifierService, ['raiseError'] )
        }
      ]
    } );

    errorNotifierService = TestBed.inject( ErrorNotifierService ) as jasmine.SpyObj<ErrorNotifierService>
    interceptor = TestBed.inject( ServerErrorHandlingInterceptor );
  } );

  it( 'should be created', () => {
    expect( interceptor ).toBeTruthy();
  } );

  it( 'should notify ErrorNotifierService', () => {
    // arrange
    const request = new HttpRequest( 'POST', '/test', null );
    const response = new HttpErrorResponse( { status: 500, statusText: 'Internal Server Error' } );
    const error = new HttpError( response );

    // act
    interceptor.intercept( request, next ).subscribe( { error: _ => { } } );
    nextSubject.error( error );

    // assert
    expect( errorNotifierService.raiseError ).toHaveBeenCalled();

  } );

  it( 'should throw ServerError on 500 Internal Server Error',
    describeError( 500, 'Internal Server Error', ( error: ServerError, problemDetails, _response ) => {
      expect( error ).toBeInstanceOf( ServerError )
      expect( error.message ).toBe( problemDetails.detail as string )
    } )
  );

  it( 'should throw ProxyError on 502 Bad Gateway',
    describeError( 502, 'Bad Gateway', ( error: ProxyError, problemDetails, _response ) => {
      expect( error ).toBeInstanceOf( ProxyError )
      expect( error.message ).toBe( problemDetails.detail as string )
    } )
  );

  it( 'should throw TechnicalBreakError on 503 Service Unavailable',
    describeError( 503, 'Service Unavailable', ( error: TechnicalBreakError, problemDetails, _response ) => {
      expect( error ).toBeInstanceOf( TechnicalBreakError )
      expect( error.message ).toBe( problemDetails.detail as string )
    } )
  );

  it( 'should throw ProxyTimeoutError on 504 Gateway Timeout',
    describeError( 504, 'Gateway Timeout', ( error: ProxyTimeoutError, problemDetails, _response ) => {
      expect( error ).toBeInstanceOf( ProxyTimeoutError )
      expect( error.message ).toBe( problemDetails.detail as string )
    } )
  );

  function describeError(
    status: number,
    statusText: string,
    checkError: ( error: any, problemDetails: ProblemDetails, response: HttpErrorResponse ) => void ) {
    return () => {
      // arrange
      const request = new HttpRequest( 'POST', '/test', null );
      const problemDetails = new ProblemDetails( { status: status, detail: 'test detail' } );
      const response = new HttpErrorResponse( { status: status, statusText: statusText, error: problemDetails } );

      const error = new HttpProblem( response, problemDetails );
      let handler: Observer<HttpEvent<unknown>> = {
        complete: () => { },
        next: _ => { },
        error: error => checkError( error, problemDetails, response ),
      };

      spyOnAllFunctions( handler );

      // act
      interceptor.intercept( request, next ).subscribe( handler );
      nextSubject.error( error );

      // assert
      expect( handler.next ).not.toHaveBeenCalled();
      expect( handler.error ).toHaveBeenCalled();
    }
  }
} );

