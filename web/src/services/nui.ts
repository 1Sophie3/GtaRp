// src/web/src/services/nui.ts

declare function GetParentResourceName(): string;

export const isFiveM = (): boolean => {
  return typeof GetParentResourceName === 'function';
};

/**
 * Sendet ein NUI Callback an das FiveM-Backend.
 * Fängt im Browser/Dev-Modus Fehler sauber ab und loggt sie nur.
 */
export const sendNuiCallback = async <T = any>(
  eventName: string, 
  data: Record<string, unknown> = {}
): Promise<{ success: boolean; data?: T; error?: string }> => {
  if (!isFiveM()) {
    console.log(`[Dev Mock NUI Callback] -> Event: "${eventName}"`, data);
    return { success: true };
  }

  try {
    const resourceName = GetParentResourceName();
    const response = await fetch(`https://${resourceName}/${eventName}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json; charset=UTF-8' },
      body: JSON.stringify(data)
    });
    const result = await response.json();
    return { success: true, data: result };
  } catch (err) {
    console.warn(`[NUI Error] Serververbindung fehlgeschlagen für: ${eventName}`, err);
    return { success: false, error: 'Verbindung zum Server fehlgeschlagen.' };
  }
};