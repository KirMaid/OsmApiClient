import React from 'react';
import classes from "./Sidebar.module.scss";
import Form from 'react-bootstrap/Form';

const Sidebar = ({cities, currentCity, selectCity, places, selectPlace, currentMethod, methods, selectMethod}) => {


    return (
        <header className={classes.sidebar}>
            <div className={classes.sidebar__item}>
                <Form.Select value={currentCity} onChange={e => selectCity(e.target.value)}>
                    {cities.map(city => <option key={city} value={city}>{city}</option>)}
                </Form.Select>
            </div>
            <div className={[classes.sidebar__item, classes.sidebar__company].join(' ')}>
                <p className={classes.sidebar__title}>Заведения</p>
                {places.map((place, index) =>
                    <Form.Check
                        className={classes.sidebar__option}
                        checked={place.checked}
                        key={place.name}
                        type={'checkbox'}
                        id={place.name}
                        label={place.name}
                        onChange={() => selectPlace(index)}
                    />
                )}
            </div>
            <div className={classes.sidebar__item}>
                <p className={classes.sidebar__title}>Метод</p>
                {methods.map(method =>
                    <Form.Check
                        className={classes.sidebar__option}
                        key={method}
                        checked={currentMethod === method}
                        type='radio'
                        label={method}
                        name='method'
                        id={method}
                        onChange={e => selectMethod(method)}
                    />
                )}
            </div>
        </header>
    );
};

export default Sidebar;