// src/types/garage.ts
// Gemeinsamer Datenvertrag für die Garage-UI. Wird sowohl von GarageMenu.vue
// als auch (perspektivisch) von jeder Backend-Anbindung (FiveM heute, ggf.
// ein anderes Framework später) genutzt, damit sich Payload-Änderungen nur
// an einer Stelle niederschlagen.

export interface GarageVehicle {
  Id: number;
  ModelName: string;
  NumberPlate: string;
  /** Health wird serverseitig 0-1000 geliefert, UI zeigt es als Prozent (Health / 10). */
  Health: number;
}

export interface GarageOpenPayload {
  inGarageVehicles: GarageVehicle[];
  nearbyParkableVehicles: GarageVehicle[];
  maxCapacity: number;
}