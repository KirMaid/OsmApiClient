import React, {useState} from 'react';
import './App.css'
import Heatmap from "./components/Heatmap";
import Sidebar from "./components/Sidebar/Sidebar";
//import { useSearchParams } from "react-router-dom";


const App = () => {
    //const [searchParams] = useSearchParams();


    const [cities, setCities] = useState([
        'Волгоград',
        'Москва',
        'Санкт-Петербург'
    ]);

    //const setCities =


    const [places, setPlaces] = useState([
        {name: 'Продуктовые магазины', checked: false},
        {name: 'Больницы', checked: false},
        { name: 'Школы', checked: true },
        { name: 'Детские сады', checked: false },
        { name: 'Остановки', checked: false },
    ]);
    const [methods, setMethods] = useState([
        'Стандартный расчёт',
        'Реверс'
    ]);

    const [currentCity, setCurrentCity] = useState('Волгоград');
    const [currentMethod, setCurrentMethod] = useState('Стандартный расчёт');

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