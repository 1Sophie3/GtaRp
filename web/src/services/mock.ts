// src/services/mock.ts

export function initMockEnvironment() {
  if (import.meta.env.DEV) {
    console.log('[Dev Mock] AdminPanel Mock-Umgebung ist aktiv.');

    // F3 Taste simuliert das Öffnen des Admin-Panels aus FiveM
    window.addEventListener('keydown', (event: KeyboardEvent) => {
      if (event.key === 'F3') {
        window.postMessage({
          action: 'openAdminPanel',
          payload: {
            adminLevel: 5,
            ownAccountId: 1,
            players: [
              { id: 1, name: 'Admin_Master (Du)' },
              { id: 2, name: 'Max_Mustermann' },
              { id: 3, name: 'Erika_Musterfrau' },
              { id: 4, name: 'Hans_Peter' }
            ],
            houses: [
              { id: 101, name: 'Villa Vinewood', owner: 'Max_Mustermann' },
              { id: 102, name: 'Appartement Sandy Shores', owner: 'Staat' }
            ],
            tpLocations: ['Legion Square', 'Würfelpark', 'LSPD Headquarters', 'Flughafen'],
            tickets: [
              {
                Id: 1,
                ReporterId: 2,
                ReporterName: 'Max_Mustermann',
                Message: 'Mein Auto steckt im Boden fest!',
                IsPriority: true,
                AdminComment: []
              },
              {
                Id: 2,
                ReporterId: 3,
                ReporterName: 'Erika_Musterfrau',
                Message: 'Ich brauche Hilfe bei einem Bug im Shop.',
                IsPriority: false,
                AdminComment: []
              }
            ]
          }
        }, '*');
      }
    });
  }
}

// src/services/mock.ts (oder in deinem DevControlPanel)
export function triggerAdminPanelMock() {
  window.postMessage({
    action: 'openAdminPanel',
    payload: {
      adminLevel: 5,
      ownAccountId: 1,
      players: [
        { id: 1, name: 'Dev_Admin (Du)' },
        { id: 2, name: 'Max_Mustermann' },
        { id: 3, name: 'Erika_Musterfrau' }
      ],
      houses: [
        { id: 101, name: 'Villa Vinewood', owner: 'Max_Mustermann' },
        { id: 102, name: 'Appartement Sandy', owner: 'Staat' }
      ],
      tpLocations: ['Legion Square', 'Würfelpark', 'LSPD', 'Flughafen'],
      tickets: [
        {
          Id: 1,
          ReporterId: 2,
          ReporterName: 'Max_Mustermann',
          Message: 'Mein Auto ist weg!',
          IsPriority: true,
          AdminComment: []
        }
      ]
    }
  }, '*');
}

// In Dev-Umgebung automatisch beim Drücken von F3 oder Button auslösen
if (import.meta.env.DEV) {
  window.addEventListener('keydown', (e) => {
    if (e.key === 'F3') {
      triggerAdminPanelMock();
    }
  });
}