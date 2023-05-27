import React, {useState} from 'react';
import './App.css'
import Heatmap from "./components/Heatmap";
import Sidebar from "./components/Sidebar/Sidebar";


const App = () => {
    const [cities, setCities] = useState([
        'Волгоград',
        'Москва',
        'Санкт-Петербург'
    ]);

    //const setCities =


    const [places, setPlaces] = useState([
        {name: 'Бары', checked: false},
        {name: 'Кафе', checked: false},
        {name: 'Рестораны', checked: false},
    ]);
    const [methods, setMethods] = useState([
        'Метод N',
        'Метод M'
    ]);

    const [currentCity, setCurrentCity] = useState('Волгоград');
    const [currentMethod, setCurrentMethod] = useState('Метод M');

    const selectCity = (city) => {
        setCurrentCity(city)
    }

    const selectPlace = (place) => {
        setPlaces((prevPlaces) => {
            const updatedPlaces = [...prevPlaces];
            updatedPlaces[place].checked = !updatedPlaces[place].checked;
            return updatedPlaces
        })
    }

    const selectMethod = (method) => {
        setCurrentMethod(method)
    }

    return (
        <div>
            <Sidebar
                cities={cities}
                currentCity={currentCity}
                selectCity={selectCity}
                places={places}
                currentPlace={selectPlace}
                selectPlace={selectPlace}
                methods={methods}
                currentMethod={currentMethod}
                selectMethod={selectMethod}/>
            <Heatmap currentCity={currentCity} currentPlace={selectPlace} currentMethod={currentMethod}/>
        </div>
    );
};

export default App;