# Registro de Portería

Aplicación de escritorio en C# / Windows Forms para controlar los **ingresos
y salidas** de un conjunto residencial desde la portería.

## 1. Problema que resuelve

El portero necesita llevar un control de quién entra y quién sale del
edificio, diferenciando dos tipos de personas:

- **Residente**: vive en el edificio y tiene un apartamento asignado.
- **Visitante**: viene de afuera, va dirigido a un apartamento y debe estar
  autorizado por alguien (normalmente el residente al que visita).

Por cada movimiento se registra: quién es, si es entrada o salida, la fecha
y hora, y observaciones. La aplicación valida los datos, evita
inconsistencias (por ejemplo, registrar una salida sin una entrada previa),
lleva estadísticas de lo ocurrido y guarda todo en un archivo de texto para
que la información no se pierda al cerrar el programa.

## 2. Funcionalidades principales

- **Registro de acceso**: formulario para registrar entradas/salidas de
  residentes y visitantes, con una tabla que muestra todos los movimientos.
- **Reportes**: genera un informe con totales (entradas, salidas,
  residentes, visitantes, registros rechazados, personas que siguen dentro)
  y una matriz de movimientos por día de la semana y tipo de persona.
- **Persistencia**: guarda y carga los registros desde un archivo de texto
  (`registros_porteria.txt` por defecto). Además de los botones manuales,
  la app carga automáticamente al iniciar y guarda automáticamente al
  cerrarse, para que los datos nunca se pierdan.

## 3. Cómo se cumple cada requisito

| # | Requisito | Dónde está |
|---|-----------|------------|
| 1 | GUI por código, 3 pestañas, tabla, eventos | `UI/VentanaPrincipal.cs` — `TabControl` con 3 `TabPage` construidas 100% por código (sin diseñador), `DataGridView` (`grilla`), eventos `Click`/`SelectedIndexChanged`/`FormClosing` (prefijo `Al...`) |
| 2 | Validación con mensajes, if/switch | La validación real vive en `Validador.Datos()` (lógica, no en la ventana); la ventana solo muestra el mensaje con `ErrorProvider` (`errores`) y las etiquetas de estado. Decisiones con `if`/`switch` en `Validador` y `RegistroAccesos.Registrar` |
| 3 | 3+ clases con constructor, herencia + polimorfismo, UML | `Persona` (abstracta) → `Residente` / `Visitante`, cada una sobrescribe `Tipo` y `Detalle`. Diagrama en la sección 5 |
| 4 | Lógica separada de la UI | Todo en `Modelo/` y `Logica/` — ninguna de esas clases hace `using System.Windows.Forms` |
| 5 | Vector/lista y matriz | `BindingList<RegistroAcceso>` en `RegistroAccesos` (se enlaza directo a la grilla); matriz `int[7,2]` de conteos por día de la semana x tipo de persona |
| 6 | Informe con contadores, acumuladores, banderas | `RegistroAccesos.Informe()`: contadores (`entradas`, `residentes`, ...), acumulador (`total`), bandera (`hayGenteAdentro`) |
| 7 | Persistencia en archivo de texto | `Logica/Archivo.cs` + `RegistroAcceso.ToLinea()/DesdeLinea()`, formato delimitado por `;`. Se guarda al cerrar y se carga al abrir (`registros_porteria.txt`), y también hay diálogos `Guardar como.../Abrir archivo...` |
| 8 | Repositorio GitHub con commits de ambos integrantes | Ver sección 6 |

## 4. Estructura del proyecto

```
RegistroPorteria/
├── RegistroPorteria.sln
├── README.md
└── PorteriaApp/
    ├── PorteriaApp.csproj
    ├── Program.cs                     # Punto de entrada (Main)
    ├── Modelo/                        # Clases del problema (sin Forms)
    │   ├── Persona.cs                 # Clase abstracta base
    │   ├── Residente.cs               # Hereda de Persona
    │   ├── Visitante.cs               # Hereda de Persona
    │   ├── RegistroAcceso.cs          # Un movimiento; ToLinea()/DesdeLinea()
    │   └── TipoMovimiento.cs          # enum Entrada/Salida
    ├── Logica/                        # Lógica de negocio (sin Forms)
    │   ├── RegistroAccesos.cs         # BindingList, matriz, Registrar(), Informe()
    │   ├── Validador.cs               # Validación de datos (estático)
    │   └── Archivo.cs                 # Guardar/cargar en archivo de texto (estático)
    └── UI/
        └── VentanaPrincipal.cs        # Única clase que usa Windows Forms
```

## 5. Diagrama UML de clases

```mermaid
classDiagram
    class Persona {
        <<abstract>>
        +string Nombre
        +string Documento
        +string Tipo*
        +string Detalle*
        #Persona(nombre, documento)
    }
    class Residente {
        +string Apartamento
        +Residente(nombre, documento, apartamento)
        +Tipo string
        +Detalle string
    }
    class Visitante {
        +string ApartamentoDestino
        +string AutorizadoPor
        +Visitante(nombre, documento, apartamentoDestino, autorizadoPor)
        +Tipo string
        +Detalle string
    }
    class TipoMovimiento {
        <<enumeration>>
        Entrada
        Salida
    }
    class RegistroAcceso {
        +int Id
        +Persona Persona
        +TipoMovimiento Movimiento
        +DateTime FechaHora
        +string Observaciones
        +RegistroAcceso(id, persona, movimiento, fechaHora, observaciones)
        +ToLinea() string
        +DesdeLinea(linea)$ RegistroAcceso
    }
    class RegistroAccesos {
        -BindingList~RegistroAcceso~ registros
        -int[,] matriz
        -int rechazados
        +Registrar(persona, movimiento, fecha, observaciones) string
        +EstaDentro(documento) bool
        +Reemplazar(nuevos)
        +Informe() string
    }
    class Validador {
        <<static>>
        +Datos(persona)$ string
    }
    class Archivo {
        <<static>>
        +Guardar(registros, ruta)$
        +Cargar(ruta)$ List~RegistroAcceso~
    }
    class VentanaPrincipal {
        -RegistroAccesos accesos
        +VentanaPrincipal()
    }

    Persona <|-- Residente
    Persona <|-- Visitante
    RegistroAcceso --> Persona : compone
    RegistroAcceso --> TipoMovimiento
    RegistroAccesos "1" --> "*" RegistroAcceso : administra
    RegistroAccesos --> Validador : usa
    VentanaPrincipal --> RegistroAccesos : usa
    VentanaPrincipal --> Archivo : usa
```

> GitHub renderiza este diagrama automáticamente al ver el README en el
> repositorio (no hace falta abrir nada más).

## 6. Cómo ejecutarlo

Windows Forms solo se **ejecuta** en Windows. Se puede **compilar** en
cualquier sistema operativo con el SDK de .NET 8, pero para correr la
aplicación y ver la ventana se necesita Windows.

### Opción A: Visual Studio (recomendada)

1. Instalar **Visual Studio 2022** con la carga de trabajo
   *"Desarrollo de escritorio de .NET"*.
2. Clonar el repositorio y abrir `RegistroPorteria.sln`.
3. Presionar `F5` (o el botón ▶ "PorteriaApp") para compilar y ejecutar.

### Opción B: línea de comandos (Windows, con el SDK de .NET 8 instalado)

```bash
git clone <URL-del-repositorio>
cd RegistroPorteria/PorteriaApp
dotnet run
```

Al cerrar la aplicación, los registros quedan guardados automáticamente en
`PorteriaApp/registros_porteria.txt` (se crea la primera vez que se guarda
o se cierra el programa).

## 7. Autores

- Integrante 1: _(completar)_
- Integrante 2: _(completar)_
