import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CounterComponent } from './counter.component';

describe( 'CounterComponent', () => {
  let component: CounterComponent;
  let fixture: ComponentFixture<CounterComponent>;

  beforeEach( async () => {
    await TestBed.configureTestingModule( {
      declarations: [CounterComponent]
    } )
      .compileComponents();

    fixture = TestBed.createComponent( CounterComponent );
    component = fixture.componentInstance;
    fixture.detectChanges();
  } );

  it( 'should create', () => {
    expect( component ).toBeTruthy();
  } );

  it( 'should show count', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect( compiled.querySelector( '.counter p' )?.textContent )
      .toContain( `Clicked ${component.count} times` );
  } )

  it( 'should increment count on click', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    var button = compiled.querySelector( '.counter button' ) as HTMLButtonElement
    var prev = component.count;
    button.click();
    expect( component.count ).toBe( prev + 1 );
  } )
} );
