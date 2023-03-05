import { InjectionToken } from '@angular/core'

export type FileSaver = ( blob: Blob, filename: string ) => void

export const FILESAVER = new InjectionToken<FileSaver>( 'fileSaver' )

export function getFileSaver(): FileSaver {
    return saveFile;
}

function saveFile( blob: Blob, fileName: string ) {
    const a = document.createElement( 'a' )
    const objectUrl = URL.createObjectURL( blob )
    a.href = objectUrl
    a.download = fileName;
    a.click();
    URL.revokeObjectURL( objectUrl )
}