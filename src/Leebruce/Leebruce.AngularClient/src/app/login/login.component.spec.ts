import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { of, Subject } from 'rxjs';
import { AuthenticationService } from '../api/authentication/authentication.service';

import { LoginComponent } from './login.component';

describe( 'LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let router: Router;
  let route: ActivatedRoute;
  let authService: jasmine.SpyObj<AuthenticationService>;
  let routeSubject = new Subject<any>();
  let html: HTMLElement;

  beforeEach( async () => {
    await TestBed.configureTestingModule( {
      declarations: [LoginComponent],
      imports: [
        RouterTestingModule,
        FormsModule
      ],
      providers: [
        {
          provide: AuthenticationService,
          useValue: jasmine.createSpyObj( typeof AuthenticationService, ['logIn'] )
        }
      ]
    } )
      .compileComponents();

    route = TestBed.inject( ActivatedRoute )
    router = TestBed.inject( Router )
    authService = TestBed.inject( AuthenticationService ) as jasmine.SpyObj<AuthenticationService>

    spyOnProperty( route, 'queryParamMap', 'get' ).and.returnValue( routeSubject.asObservable() )

    fixture = TestBed.createComponent( LoginComponent );
    component = fixture.componentInstance;
    fixture.detectChanges();
    html = fixture.debugElement.nativeElement;
  } );

  it( 'should create', () => {
    expect( component ).toBeTruthy();
  } );

  it( 'should redirect home on complete', () => {
    // arrange
    const navigateSpy = spyOn( router, 'navigate' );
    component.form.username = 'user'
    component.form.password = 'pass';

    authService.logIn.and.returnValue( of() )

    // act
    component.onSubmit()

    // assert
    expect( navigateSpy ).toHaveBeenCalledWith( [''] );

    expect( component ).toBeTruthy();
  } );

  it( 'should redirect back on complete if path provided', () => {
    // arrange
    const navigateSpy = spyOn( router, 'navigate' );
    component.form.username = 'user'
    component.form.password = 'pass';

    authService.logIn.and.returnValue( of() )
    routeSubject.next( convertToParamMap( { 'redirect': 'hello' } ) )

    // act
    component.onSubmit()

    // assert
    expect( navigateSpy ).toHaveBeenCalledWith( ['hello'] );

    expect( component ).toBeTruthy();
  } );

  it( 'Should show that fields are required', fakeAsync( () => {
    // arrange
    let submitSpy = spyOn( component, 'onSubmit' );
    let button = html.querySelector( 'button.submit' ) as HTMLElement

    // act
    button.click();
    fixture.detectChanges();
    tick();

    // assert
    let usernameValidationBox = html.querySelector( 'input[name="username"] + .input-validation' ) as HTMLElement
    expect( usernameValidationBox ).not.toBeNull();
    expect( usernameValidationBox.innerText ).toContain( 'Username is required.' );

    let passwordValidationBox = html.querySelector( 'input[name="password"] + .input-validation' ) as HTMLElement
    expect( passwordValidationBox ).not.toBeNull();
    expect( passwordValidationBox.innerText ).toContain( 'Password is required.' );

    expect( submitSpy ).not.toHaveBeenCalled()
  } ) );

} );
