import React, {useState} from 'react';
import classes from './My-Select.module.scss'

const MySelect = ({value, options, select, checkBox}) => {
    const [optionsVisible, setOptionsVisible] = useState(false);

    const selectOption = (opt) => {
        select(opt)
    }
    const selectCheckBox = (opt) => {
        select(opt)
    }

    return (
        <div className={classes.select}
             onMouseEnter={() => setOptionsVisible(true)}
             onMouseLeave={() => setOptionsVisible(false)}>
            <div
                className={classes.current}
            >{ checkBox ? 'Заведения' : value }</div>
            {
                optionsVisible &&
                <div className={classes.options}>
                    {options.map((opt, index) =>
                        <div key={index} className={classes.options__item}>
                            {
                                checkBox
                                    ?
                                    <div>
                                        <span>{opt.name}</span><input onChange={() => selectCheckBox(index)} checked={opt.checked} type='checkbox'/>
                                    </div>
                                    :
                                    <p onClick={() => selectOption(opt)}>
                                        {opt}
                                    </p>
                            }
                        </div>
                    )}
                </div>
            }
        </div>
    );
};

export default MySelect;