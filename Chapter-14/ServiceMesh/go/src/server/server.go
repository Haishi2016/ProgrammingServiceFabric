package main

import (
	"net/http"
	"html/template"
	"time"
	"database/sql"
	"fmt"
	_ "github.com/lib/pq"
        "os"
        "net"
        "log"
)

const (
	DB_USER = "user"
	DB_PASSWORD = "password"
	DB_NAME = "pet_db"
	DB_PORT = 5432
)

type NullTime struct {
	Time time.Time
	Valid bool
}

func (nt *NullTime) Scan(value interface{}) error {
	nt.Time, nt.Valid = value.(time.Time)
	return nil
}

func (nt NullTime) String() string {
	if nt.Valid {
		return nt.Time.Format(time.RFC3339)
	} else {
		return "--"
	}
}

type Pet struct {
	Name string
	Owner string
	Species	string
	Sex string 
	Birth NullTime 
	Death NullTime
}

type PetsPage struct {
	Pets map[string]Pet
}

func indexHandler(w http.ResponseWriter, r *http.Request) {
        log.Println("Environment variable POSTGRES_HOST=", os.Getenv("POSTGRES_HOST"))
        dbAddr, _ := net.LookupHost(os.Getenv("POSTGRES_HOST"))
        log.Println("First address:", dbAddr[0])
	dbinfo := fmt.Sprintf("host=%s port=%d user=%s password=%s dbname=%s sslmode=disable",
		dbAddr[0], DB_PORT, DB_USER, DB_PASSWORD, DB_NAME)
	db, err := sql.Open("postgres", dbinfo)
	if  err != nil {
		log.Println("Failed to connect:", err)
	}
	defer db.Close()
	rows, _ := db.Query("SELECT * FROM pet")
	log.Println("Queried Row:", rows)
	pets := make(map[string]Pet)
	for rows.Next() {
		var pet Pet
		_ = rows.Scan(&pet.Name, &pet.Owner, &pet.Species, &pet.Sex, &pet.Birth, &pet.Death)
		pets[pet.Name] = pet
	}
	t, _ := template.ParseFiles("index.html")
	p := PetsPage{ Pets: pets}
	t.Execute(w, p)
}

func main() {
	http.HandleFunc("/", indexHandler)
	http.ListenAndServe(":8084", nil)
}
